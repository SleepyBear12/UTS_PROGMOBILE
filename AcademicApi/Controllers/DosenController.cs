using AcademicApi.Data;
using AcademicApi.DTOs;
using AcademicApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcademicApi.Controllers;

[ApiController]
[Route("api/dosen")]
[Produces("application/json")]
public class DosenController : ControllerBase
{
    private readonly AcademicDbContext _context;

    public DosenController(AcademicDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Mengambil seluruh data dosen.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DosenDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DosenDto>>> GetAll(CancellationToken cancellationToken)
    {
        var data = await _context.Dosen
            .AsNoTracking()
            .OrderBy(d => d.DosenId)
            .Select(d => new DosenDto
            {
                DosenId = d.DosenId,
                Nidn = d.Nidn,
                NamaDosen = d.NamaDosen,
                Email = d.Email,
                Prodi = d.Prodi
            })
            .ToListAsync(cancellationToken);

        return Ok(data);
    }

    /// <summary>
    /// Mengambil satu data dosen berdasarkan ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(DosenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DosenDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var data = await _context.Dosen
            .AsNoTracking()
            .Where(d => d.DosenId == id)
            .Select(d => new DosenDto
            {
                DosenId = d.DosenId,
                Nidn = d.Nidn,
                NamaDosen = d.NamaDosen,
                Email = d.Email,
                Prodi = d.Prodi
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
        {
            return NotFound(new ErrorResponse { Message = "Dosen tidak ditemukan." });
        }

        return Ok(data);
    }

    /// <summary>
    /// Menambahkan data dosen baru.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(DosenDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DosenDto>> Create(
        [FromBody] DosenRequestDto request,
        CancellationToken cancellationToken)
    {
        if (await _context.Dosen.AsNoTracking()
            .AnyAsync(d => d.Nidn == request.Nidn, cancellationToken))
        {
            return Conflict(new ErrorResponse
            {
                Message = "NIDN sudah digunakan oleh dosen lain."
            });
        }

        var dosen = new Dosen
        {
            Nidn = request.Nidn,
            NamaDosen = request.NamaDosen,
            Email = request.Email,
            Prodi = request.Prodi
        };

        _context.Dosen.Add(dosen);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Pengaman tambahan bila ada request lain menyimpan NIDN yang sama.
            if (await _context.Dosen.AsNoTracking()
                .AnyAsync(d => d.Nidn == request.Nidn, cancellationToken))
            {
                return Conflict(new ErrorResponse
                {
                    Message = "NIDN sudah digunakan oleh dosen lain."
                });
            }

            throw;
        }

        var result = new DosenDto
        {
            DosenId = dosen.DosenId,
            Nidn = dosen.Nidn,
            NamaDosen = dosen.NamaDosen,
            Email = dosen.Email,
            Prodi = dosen.Prodi
        };

        return CreatedAtAction(nameof(GetById), new { id = dosen.DosenId }, result);
    }

    /// <summary>
    /// Mengubah data dosen berdasarkan ID.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] DosenRequestDto request,
        CancellationToken cancellationToken)
    {
        var dosen = await _context.Dosen
            .FirstOrDefaultAsync(d => d.DosenId == id, cancellationToken);

        if (dosen is null)
        {
            return NotFound(new ErrorResponse { Message = "Dosen tidak ditemukan." });
        }

        if (await _context.Dosen.AsNoTracking()
            .AnyAsync(d => d.Nidn == request.Nidn && d.DosenId != id, cancellationToken))
        {
            return Conflict(new ErrorResponse
            {
                Message = "NIDN sudah digunakan oleh dosen lain."
            });
        }

        dosen.Nidn = request.Nidn;
        dosen.NamaDosen = request.NamaDosen;
        dosen.Email = request.Email;
        dosen.Prodi = request.Prodi;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            if (await _context.Dosen.AsNoTracking()
                .AnyAsync(d => d.Nidn == request.Nidn && d.DosenId != id, cancellationToken))
            {
                return Conflict(new ErrorResponse
                {
                    Message = "NIDN sudah digunakan oleh dosen lain."
                });
            }

            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Menghapus dosen jika tidak lagi menjadi dosen wali.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var dosen = await _context.Dosen
            .FirstOrDefaultAsync(d => d.DosenId == id, cancellationToken);

        if (dosen is null)
        {
            return NotFound(new ErrorResponse { Message = "Dosen tidak ditemukan." });
        }

        var jumlahMahasiswa = await _context.Mahasiswa
            .CountAsync(m => m.DosenId == id, cancellationToken);

        if (jumlahMahasiswa > 0)
        {
            return Conflict(new ErrorResponse
            {
                Message = $"Dosen tidak dapat dihapus karena masih menjadi dosen wali bagi {jumlahMahasiswa} mahasiswa."
            });
        }

        _context.Dosen.Remove(dosen);

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // Pengaman bila mahasiswa baru ditambahkan tepat sebelum proses hapus.
            return Conflict(new ErrorResponse
            {
                Message = "Dosen tidak dapat dihapus karena masih menjadi dosen wali bagi mahasiswa."
            });
        }

        return NoContent();
    }
}
