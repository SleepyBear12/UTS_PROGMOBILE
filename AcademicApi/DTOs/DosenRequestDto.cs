using System.ComponentModel.DataAnnotations;

namespace AcademicApi.DTOs;

public class DosenRequestDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "NIDN wajib diisi.")]
    [StringLength(20, ErrorMessage = "NIDN maksimal 20 karakter.")]
    public string Nidn { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Nama dosen wajib diisi.")]
    [StringLength(100, ErrorMessage = "Nama dosen maksimal 100 karakter.")]
    public string NamaDosen { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email wajib diisi.")]
    [StringLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    public string Email { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Program studi wajib diisi.")]
    [StringLength(100, ErrorMessage = "Program studi maksimal 100 karakter.")]
    public string Prodi { get; set; } = string.Empty;
}
