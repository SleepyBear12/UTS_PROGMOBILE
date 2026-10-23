namespace AcademicApi.DTOs;

public class MahasiswaDto
{
    public int MahasiswaId { get; set; }

    public string Nim { get; set; } = string.Empty;

    public string NamaMahasiswa { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int Angkatan { get; set; }

    public int DosenId { get; set; }

    public string NamaDosenWali { get; set; } = string.Empty;
}
