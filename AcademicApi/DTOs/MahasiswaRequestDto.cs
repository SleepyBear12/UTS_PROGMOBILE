using System.ComponentModel.DataAnnotations;

namespace AcademicApi.DTOs;

public class MahasiswaRequestDto
{
    [Required(AllowEmptyStrings = false, ErrorMessage = "NIM wajib diisi.")]
    [StringLength(20, ErrorMessage = "NIM maksimal 20 karakter.")]
    public string Nim { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Nama mahasiswa wajib diisi.")]
    [StringLength(100, ErrorMessage = "Nama mahasiswa maksimal 100 karakter.")]
    public string NamaMahasiswa { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Email wajib diisi.")]
    [StringLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
    [EmailAddress(ErrorMessage = "Format email tidak valid.")]
    public string Email { get; set; } = string.Empty;

    [Range(AngkatanRule.Min, AngkatanRule.Max,
        ErrorMessage = "Angkatan harus berada di antara 2020 dan 2026.")]
    public int Angkatan { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Dosen wali wajib dipilih.")]
    public int DosenId { get; set; }
}
