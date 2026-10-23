using AcademicApi.Data;
using AcademicApi.DTOs;
using AcademicApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicApi.Controllers;

[ApiController]
[Route("api/mahasiswa")]
[Produces("application/json")]
public class MahasiswaController : ControllerBase
{
    private readonly AcademicDbContext _context;

    public MahasiswaController(AcademicDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Mengambil seluruh data mahasiswa beserta nama dosen wali.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MahasiswaDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MahasiswaDto>>> GetAll(CancellationToken cancellationToken)
    {
        var data = await _context.Mahasiswa
            .AsNoTracking()
            .OrderBy(m => m.MahasiswaId)
            .Select(m => new MahasiswaDto
            {
                MahasiswaId = m.MahasiswaId,
                Nim = m.Nim,
                NamaMahasiswa = m.NamaMahasiswa,
                Email = m.Email,
                Angkatan = m.Angkatan,
                DosenId = m.DosenId,
                NamaDosenWali = m.Dosen.NamaDosen
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    /// <summary>
    /// Mengambil detail satu mahasiswa berdasarkan ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MahasiswaDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MahasiswaDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var data = await _context.Mahasiswa
            .AsNoTracking()
            .Where(m => m.MahasiswaId == id)
            .Select(m => new MahasiswaDto
            {
                MahasiswaId = m.MahasiswaId,
                Nim = m.Nim,
                NamaMahasiswa = m.NamaMahasiswa,
                Email = m.Email,
                Angkatan = m.Angkatan,
                DosenId = m.DosenId,
                NamaDosenWali = m.Dosen.NamaDosen
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return NotFound(new ErrorResponse { Message = "Mahasiswa tidak ditemukan." });
        }

        return Ok(data);
    }

    /// <summary>
    /// Menambahkan data mahasiswa baru beserta dosen wali.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MahasiswaDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MahasiswaDto>> Create(
        [FromBody] MahasiswaRequestDto request,
        CancellationToken cancellationToken)
    {
        var dosen = await _context.Dosen.AsNoTracking()
            .FirstOrDefaultAsync(d => d.DosenId == request.DosenId, cancellationToken);

        if (dosen is null)
        {
            return BadRequest(new ErrorResponse { Message = "Dosen wali tidak ditemukan." });
        }

        if (await _context.Mahasiswa.AsNoTracking()
            .AnyAsync(m => m.Nim == request.Nim, cancellationToken))
        {
            return Conflict(new ErrorResponse
            {
                Message = "NIM sudah digunakan oleh mahasiswa lain."
            });
        }

        var mahasiswa = new Mahasiswa
        {
            Nim = request.Nim,
            NamaMahasiswa = request.NamaMahasiswa,
            Email = request.Email,
            Angkatan = request.Angkatan,
            DosenId = request.DosenId
        };

        _context.Mahasiswa.Add(mahasiswa);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Pengaman tambahan bila ada request lain menyimpan NIM yang sama.
            if (await _context.Mahasiswa.AsNoTracking()
                .AnyAsync(m => m.Nim == request.Nim, cancellationToken))
            {
                return Conflict(new ErrorResponse
                {
                    Message = "NIM sudah digunakan oleh mahasiswa lain."
                });
            }

            throw;
        }

        var result = new MahasiswaDto
        {
            MahasiswaId = mahasiswa.MahasiswaId,
            Nim = mahasiswa.Nim,
            NamaMahasiswa = mahasiswa.NamaMahasiswa,
            Email = mahasiswa.Email,
            Angkatan = mahasiswa.Angkatan,
            DosenId = mahasiswa.DosenId,
            NamaDosenWali = dosen.NamaDosen
        };

        return CreatedAtAction(nameof(GetById), new { id = mahasiswa.MahasiswaId }, result);
    }

    /// <summary>
    /// Mengubah data mahasiswa dan dosen wali berdasarkan ID.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] MahasiswaRequestDto request,
        CancellationToken cancellationToken)
    {
        var mahasiswa = await _context.Mahasiswa
            .FirstOrDefaultAsync(m => m.MahasiswaId == id, cancellationToken);

        if (mahasiswa is null)
        {
            return NotFound(new ErrorResponse { Message = "Mahasiswa tidak ditemukan." });
        }

        var dosenExists = await _context.Dosen.AsNoTracking()
            .AnyAsync(d => d.DosenId == request.DosenId, cancellationToken);

        if (!dosenExists)
        {
            return BadRequest(new ErrorResponse { Message = "Dosen wali tidak ditemukan." });
        }

        if (await _context.Mahasiswa.AsNoTracking()
            .AnyAsync(m => m.Nim == request.Nim && m.MahasiswaId != id, cancellationToken))
        {
            return Conflict(new ErrorResponse
            {
                Message = "NIM sudah digunakan oleh mahasiswa lain."
            });
        }

        mahasiswa.Nim = request.Nim;
        mahasiswa.NamaMahasiswa = request.NamaMahasiswa;
        mahasiswa.Email = request.Email;
        mahasiswa.Angkatan = request.Angkatan;
        mahasiswa.DosenId = request.DosenId;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await _context.Mahasiswa.AsNoTracking()
                .AnyAsync(m => m.Nim == request.Nim && m.MahasiswaId != id, cancellationToken))
            {
                return Conflict(new ErrorResponse
                {
                    Message = "NIM sudah digunakan oleh mahasiswa lain."
                });
            }

            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Menghapus data mahasiswa berdasarkan ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var mahasiswa = await _context.Mahasiswa
            .FirstOrDefaultAsync(m => m.MahasiswaId == id, cancellationToken);

        if (mahasiswa is null)
        {
            return NotFound(new ErrorResponse { Message = "Mahasiswa tidak ditemukan." });
        }

        _context.Mahasiswa.Remove(mahasiswa);
        await _context.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
