namespace Simprak.Core.Models;

public class RekapPraktikanDto
{
    public int IdPraktikan { get; set; }
    public string Nrp { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public int Angkatan { get; set; }
    public string MataKuliah { get; set; } = string.Empty;
    public int IdKelompok { get; set; }
    public int NomorKelompok { get; set; }
    public string NamaKelompok { get; set; } = string.Empty;
    public int ModulJatuhTempo { get; set; }
    public int ModulJatuhTempoTerkumpul { get; set; }
    public int ModulBelumKumpul { get; set; }
    public int JumlahTerlambat { get; set; }
    public int JumlahModulDinilai { get; set; }
    public double Ketuntasan { get; set; }
    public double NilaiAkhir { get; set; }
    public bool IsBerisiko { get; set; }
    public string StatusLabel => IsBerisiko ? "Berisiko" : "Aman";
    public string AlasanRisiko { get; set; } = string.Empty;
}

public class PraktikanBerisikoDto
{
    public int IdPraktikan { get; set; }
    public string Nrp { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public int Angkatan { get; set; }
    public string MataKuliah { get; set; } = string.Empty;
    public int IdKelompok { get; set; }
    public int NomorKelompok { get; set; }
    public string NamaKelompok { get; set; } = string.Empty;
    public string NamaAsisten { get; set; } = string.Empty;
    public int ModulBelumKumpul { get; set; }
    public int JumlahTerlambat { get; set; }
    public int JumlahModulDinilai { get; set; }
    public double Ketuntasan { get; set; }
    public double NilaiAkhir { get; set; }
    public bool R1 { get; set; }
    public bool R2 { get; set; }
    public bool R3 { get; set; }
    public string AlasanRisiko { get; set; } = string.Empty;
}

public class RekapKelompokDto
{
    public int IdKelompok { get; set; }
    public int NomorKelompok { get; set; }
    public string NamaKelompok { get; set; } = string.Empty;
    public int? IdAsisten { get; set; }
    public string NamaAsisten { get; set; } = "Belum Ditugaskan";
    public int JumlahAnggota { get; set; }
    public double RataKetuntasan { get; set; }
    public double RataNilaiAkhir { get; set; }
    public int JumlahBerisiko { get; set; }
}

public class StatusPengumpulanDto
{
    public int IdPraktikan { get; set; }
    public string Nrp { get; set; } = string.Empty;
    public string NamaLengkap { get; set; } = string.Empty;
    public int IdKelompok { get; set; }
    public int IdModul { get; set; }
    public string KodeModul { get; set; } = string.Empty;
    public string JudulModul { get; set; } = string.Empty;
    public string Kategori { get; set; } = string.Empty;
    public string TanggalMulai { get; set; } = string.Empty;
    public string Tenggat { get; set; } = string.Empty;
    public int Bobot { get; set; }
    public int? IdPengumpulan { get; set; }
    public string? WaktuKumpul { get; set; }
    public string? TautanTugas { get; set; }
    public double? Nilai { get; set; }
    public string? Catatan { get; set; }
    public int? IdPenilai { get; set; }
    public string? NamaPenilai { get; set; }
    public string? WaktuPenilaian { get; set; }
    public string StatusPengumpulan { get; set; } = "Belum Mengumpulkan";
    public bool SudahJatuhTempo { get; set; }
    public double RataKelompokModul { get; set; }
}

public class ModulStatusDto
{
    public int IdModul { get; set; }
    public string KodeModul { get; set; } = string.Empty;
    public string JudulModul { get; set; } = string.Empty;
    public string Kategori { get; set; } = string.Empty;
    public string TanggalMulai { get; set; } = string.Empty;
    public string Tenggat { get; set; } = string.Empty;
    public int Bobot { get; set; }
    public string StatusModul { get; set; } = "Berjalan";
    public int TotalPraktikan { get; set; }
    public int JumlahTerkumpul { get; set; }
    public int JumlahDinilai { get; set; }
    public double PersentaseKumpul => TotalPraktikan == 0 ? 0 : Math.Round((double)JumlahTerkumpul * 100.0 / TotalPraktikan, 1);
    public string ProgresLabel => $"{JumlahTerkumpul}/{TotalPraktikan} ({PersentaseKumpul:0.#}%)";
}

public class DashboardRingkasanDto
{
    public int TotalPraktikan { get; set; }
    public int TotalKelompok { get; set; }
    public int ModulAktif { get; set; }
    public double RataKetuntasanGlobal { get; set; }
    public double RataNilaiGlobal { get; set; }
    public int TotalPraktikanBerisiko { get; set; }
    public List<ChartPointDto> GrafikKetuntasan { get; set; } = new();
    public List<ChartPointDto> DistribusiNilai { get; set; } = new();
    public List<PraktikanBerisikoDto> DaftarBerisikoTeratas { get; set; } = new();
    public List<LogAktivitasViewDto> AktivitasTerbaru { get; set; } = new();
}

public class ChartPointDto
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
    public double SecondaryValue { get; set; }
    public bool IsWarning { get; set; }
}

public class LogAktivitasViewDto
{
    public int IdLog { get; set; }
    public int? IdPengguna { get; set; }
    public string NamaPengguna { get; set; } = "Sistem";
    public string Aksi { get; set; } = string.Empty;
    public string Entitas { get; set; } = string.Empty;
    public int? IdEntitas { get; set; }
    public string Keterangan { get; set; } = string.Empty;
    public string Waktu { get; set; } = string.Empty;
}

public class DetailPraktikanDto
{
    public RekapPraktikanDto Profil { get; set; } = new();
    public string NamaAsisten { get; set; } = string.Empty;
    public List<StatusPengumpulanDto> RiwayatModul { get; set; } = new();
    public List<ChartPointDto> TrenNilaiVsKelompok { get; set; } = new();
}

public class InputPenilaianBarisDto
{
    public int IdPraktikan { get; set; }
    public int IdModul { get; set; }
    public string? WaktuKumpul { get; set; }
    public string? TautanTugas { get; set; }
    public double? Nilai { get; set; }
    public string? Catatan { get; set; }
}
