using Microsoft.EntityFrameworkCore;
using Simprak.Core.Models;

namespace Simprak.Data;

public class SimprakDbContext : DbContext
{
    private readonly string _connectionString;

    public SimprakDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public SimprakDbContext(DbContextOptions<SimprakDbContext> options) : base(options)
    {
        _connectionString = string.Empty;
    }

    public DbSet<Pengguna> Pengguna => Set<Pengguna>();
    public DbSet<Kelompok> Kelompok => Set<Kelompok>();
    public DbSet<Praktikan> Praktikan => Set<Praktikan>();
    public DbSet<Modul> Modul => Set<Modul>();
    public DbSet<Pengumpulan> Pengumpulan => Set<Pengumpulan>();
    public DbSet<LogAktivitas> LogAktivitas => Set<LogAktivitas>();
    public DbSet<Pengaturan> Pengaturan => Set<Pengaturan>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured && !string.IsNullOrEmpty(_connectionString))
        {
            optionsBuilder.UseSqlite(_connectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pengguna>(e =>
        {
            e.ToTable("pengguna");
            e.HasKey(x => x.IdPengguna);
            e.Property(x => x.IdPengguna).HasColumnName("id_pengguna");
            e.Property(x => x.Username).HasColumnName("username");
            e.Property(x => x.NamaLengkap).HasColumnName("nama_lengkap");
            e.Property(x => x.PasswordHash).HasColumnName("password_hash");
            e.Property(x => x.Peran).HasColumnName("peran");
            e.Property(x => x.Tema).HasColumnName("tema");
            e.Property(x => x.StatusAktif).HasColumnName("status_aktif");
            e.Property(x => x.LoginTerakhir).HasColumnName("login_terakhir");
        });

        modelBuilder.Entity<Kelompok>(e =>
        {
            e.ToTable("kelompok");
            e.HasKey(x => x.IdKelompok);
            e.Property(x => x.IdKelompok).HasColumnName("id_kelompok");
            e.Property(x => x.NomorKelompok).HasColumnName("nomor_kelompok");
            e.Property(x => x.NamaKelompok).HasColumnName("nama_kelompok");
            e.Property(x => x.IdAsisten).HasColumnName("id_asisten");
        });

        modelBuilder.Entity<Praktikan>(e =>
        {
            e.ToTable("praktikan");
            e.HasKey(x => x.IdPraktikan);
            e.Property(x => x.IdPraktikan).HasColumnName("id_praktikan");
            e.Property(x => x.Nrp).HasColumnName("nrp");
            e.Property(x => x.NamaLengkap).HasColumnName("nama_lengkap");
            e.Property(x => x.Angkatan).HasColumnName("angkatan");
            e.Property(x => x.MataKuliah).HasColumnName("mata_kuliah");
            e.Property(x => x.IdKelompok).HasColumnName("id_kelompok");
        });

        modelBuilder.Entity<Modul>(e =>
        {
            e.ToTable("modul");
            e.HasKey(x => x.IdModul);
            e.Property(x => x.IdModul).HasColumnName("id_modul");
            e.Property(x => x.KodeModul).HasColumnName("kode_modul");
            e.Property(x => x.JudulModul).HasColumnName("judul_modul");
            e.Property(x => x.Kategori).HasColumnName("kategori");
            e.Property(x => x.TanggalMulai).HasColumnName("tanggal_mulai");
            e.Property(x => x.Tenggat).HasColumnName("tenggat");
            e.Property(x => x.Bobot).HasColumnName("bobot");
        });

        modelBuilder.Entity<Pengumpulan>(e =>
        {
            e.ToTable("pengumpulan");
            e.HasKey(x => x.IdPengumpulan);
            e.Property(x => x.IdPengumpulan).HasColumnName("id_pengumpulan");
            e.Property(x => x.IdPraktikan).HasColumnName("id_praktikan");
            e.Property(x => x.IdModul).HasColumnName("id_modul");
            e.Property(x => x.WaktuKumpul).HasColumnName("waktu_kumpul");
            e.Property(x => x.TautanTugas).HasColumnName("tautan_tugas");
            e.Property(x => x.Nilai).HasColumnName("nilai");
            e.Property(x => x.Catatan).HasColumnName("catatan");
            e.Property(x => x.IdPenilai).HasColumnName("id_penilai");
            e.Property(x => x.WaktuPenilaian).HasColumnName("waktu_penilaian");
        });

        modelBuilder.Entity<LogAktivitas>(e =>
        {
            e.ToTable("log_aktivitas");
            e.HasKey(x => x.IdLog);
            e.Property(x => x.IdLog).HasColumnName("id_log");
            e.Property(x => x.IdPengguna).HasColumnName("id_pengguna");
            e.Property(x => x.Aksi).HasColumnName("aksi");
            e.Property(x => x.Entitas).HasColumnName("entitas");
            e.Property(x => x.IdEntitas).HasColumnName("id_entitas");
            e.Property(x => x.Keterangan).HasColumnName("keterangan");
            e.Property(x => x.Waktu).HasColumnName("waktu");
        });

        modelBuilder.Entity<Pengaturan>(e =>
        {
            e.ToTable("pengaturan");
            e.HasKey(x => x.Kunci);
            e.Property(x => x.Kunci).HasColumnName("kunci");
            e.Property(x => x.Nilai).HasColumnName("nilai");
        });
    }
}
