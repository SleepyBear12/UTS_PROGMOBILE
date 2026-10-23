namespace AcademicApi.Models;

public class Mahasiswa
{
    public int MahasiswaId { get; set; }

    public string Nim { get; set; } = string.Empty;

    public string NamaMahasiswa { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public int Angkatan { get; set; }

    public int DosenId { get; set; }

    public Dosen Dosen { get; set; } = null!;
}
