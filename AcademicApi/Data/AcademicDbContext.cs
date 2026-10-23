using AcademicApi.Models;
using Microsoft.EntityFrameworkCore;

namespace AcademicApi.Data;

public class AcademicDbContext : DbContext
{
    public AcademicDbContext(DbContextOptions<AcademicDbContext> options) : base(options)
    {
    }

    public DbSet<Dosen> Dosen => Set<Dosen>();

    public DbSet<Mahasiswa> Mahasiswa => Set<Mahasiswa>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Dosen>(entity =>
        {
            entity.HasKey(d => d.DosenId);
            entity.Property(d => d.DosenId).ValueGeneratedOnAdd();
            entity.Property(d => d.Nidn).IsRequired().HasMaxLength(20);
            entity.HasIndex(d => d.Nidn).IsUnique();
            entity.Property(d => d.NamaDosen).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Email).IsRequired().HasMaxLength(100);
            entity.Property(d => d.Prodi).IsRequired().HasMaxLength(100);
        });

        modelBuilder.Entity<Mahasiswa>(entity =>
        {
            entity.HasKey(m => m.MahasiswaId);
            entity.Property(m => m.MahasiswaId).ValueGeneratedOnAdd();
            entity.Property(m => m.Nim).IsRequired().HasMaxLength(20);
            entity.HasIndex(m => m.Nim).IsUnique();
            entity.Property(m => m.NamaMahasiswa).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Email).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Angkatan).IsRequired();

            // Dosen yang masih menjadi wali tidak boleh dihapus, termasuk di level database.
            entity.HasOne(m => m.Dosen)
                .WithMany(d => d.Mahasiswa)
                .HasForeignKey(m => m.DosenId)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dosen>().HasData(
            new Dosen
            {
                DosenId = 1,
                Nidn = "0012345678",
                NamaDosen = "Dr. Anita Wijaya",
                Email = "anita.wijaya@univ.ac.id",
                Prodi = "Sistem Informasi"
            },
            new Dosen
            {
                DosenId = 2,
                Nidn = "0023456789",
                NamaDosen = "Budi Santoso, M.Kom.",
                Email = "budi.santoso@univ.ac.id",
                Prodi = "Teknik Informatika"
            },
            new Dosen
            {
                DosenId = 3,
                Nidn = "0034567890",
                NamaDosen = "Citra Lestari, M.T.",
                Email = "citra.lestari@univ.ac.id",
                Prodi = "Sistem Informasi"
            });

        modelBuilder.Entity<Mahasiswa>().HasData(
            new Mahasiswa
            {
                MahasiswaId = 1,
                Nim = "2022001001",
                NamaMahasiswa = "Andi Pratama",
                Email = "andi.pratama@student.univ.ac.id",
                Angkatan = 2022,
                DosenId = 1
            },
            new Mahasiswa
            {
                MahasiswaId = 2,
                Nim = "2022001002",
                NamaMahasiswa = "Siti Rahma",
                Email = "siti.rahma@student.univ.ac.id",
                Angkatan = 2022,
                DosenId = 1
            },
            new Mahasiswa
            {
                MahasiswaId = 3,
                Nim = "2023002001",
                NamaMahasiswa = "Rizky Hidayat",
                Email = "rizky.hidayat@student.univ.ac.id",
                Angkatan = 2023,
                DosenId = 2
            },
            new Mahasiswa
            {
                MahasiswaId = 4,
                Nim = "2024003001",
                NamaMahasiswa = "Dewi Anggraini",
                Email = "dewi.anggraini@student.univ.ac.id",
                Angkatan = 2024,
                DosenId = 2
            },
            new Mahasiswa
            {
                MahasiswaId = 5,
                Nim = "2025003001",
                NamaMahasiswa = "Fajar Nugroho",
                Email = "fajar.nugroho@student.univ.ac.id",
                Angkatan = 2025,
                DosenId = 2
            });
    }
}
