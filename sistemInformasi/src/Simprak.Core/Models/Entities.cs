namespace Simprak.Core.Models;

public class Pengguna
{
    public int IdPengguna { get; set; }
    public string Username { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Peran { get; set; } = "Asisten"; // "Admin" | "Asisten"
    public string Tema { get; set; } = "Terang";   // "Terang" | "Gelap"
    public int StatusAktif { get; set; } = 1;
    public string? LoginTerakhir { get; set; }
    public string StatusLabel => StatusAktif == 1 ? "Aktif" : "Nonaktif";
}

public class Kelompok
{
    public int IdKelompok { get; set; }
    public int NomorKelompok { get; set; }
    public string NamaKelompok { get; set; } = string.Empty;
    public int? IdAsisten { get; set; }
}

public class Praktikan
{
    public int IdPraktikan { get; set; }
    public string Nrp { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public int Angkatan { get; set; }
    public string MataKuliah { get; set; } = string.Empty;
    public int IdKelompok { get; set; }
}

public class Modul
{
    public int IdModul { get; set; }
    public string KodeModul { get; set; } = string.Empty;
    public string JudulModul { get; set; } = string.Empty;
    public string Kategori { get; set; } = "Modul Mingguan";
    public string TanggalMulai { get; set; } = string.Empty;
    public string Tenggat { get; set; } = string.Empty;
    public int Bobot { get; set; } = 10;
}

public class Pengumpulan
{
    public int IdPengumpulan { get; set; }
    public int IdPraktikan { get; set; }
    public int IdModul { get; set; }
    public string? WaktuKumpul { get; set; }
    public string? TautanTugas { get; set; }
    public double? Nilai { get; set; }
    public string? Catatan { get; set; }
    public int? IdPenilai { get; set; }
    public string? WaktuPenilaian { get; set; }
}

public class LogAktivitas
{
    public int IdLog { get; set; }
    public int? IdPengguna { get; set; }
    public string Aksi { get; set; } = string.Empty;
    public string Entitas { get; set; } = string.Empty;
    public int? IdEntitas { get; set; }
    public string Keterangan { get; set; } = string.Empty;
    public string Waktu { get; set; } = string.Empty;
}

public class Pengaturan
{
    public string Kunci { get; set; } = string.Empty;
    public string Nilai { get; set; } = string.Empty;
}
