using System.Globalization;
using System.Text.RegularExpressions;

namespace Simprak.Core.Rules;

/// <summary>
/// Implementasi Aturan Bisnis SIMPRAK (BR-01 s.d. BR-05) dan Validasi Domain.
/// Dapat diuji secara terisolasi tanpa ketergantungan UI maupun database.
/// </summary>
public static class SimprakRules
{
    public const string StatusTepatWaktu = "Tepat Waktu";
    public const string StatusTerlambat = "Terlambat";
    public const string StatusBelumMengumpulkan = "Belum Mengumpulkan";

    public const string ModulBelumDimulai = "Belum Dimulai";
    public const string ModulBerjalan = "Berjalan";
    public const string ModulSelesai = "Selesai";

    /// <summary>
    /// BR-01: Menentukan status pengumpulan berdasarkan waktu kumpul dan tenggat modul (format ISO yyyy-MM-dd HH:mm).
    /// </summary>
    public static string HitungStatusPengumpulan(string? waktuKumpul, string tenggat)
    {
        if (string.IsNullOrWhiteSpace(waktuKumpul))
            return StatusBelumMengumpulkan;

        // Perbandingan leksikografis format ISO "yyyy-MM-dd HH:mm" identik dengan perbandingan kronologis
        int cmp = string.CompareOrdinal(waktuKumpul.Trim(), tenggat.Trim());
        return cmp <= 0 ? StatusTepatWaktu : StatusTerlambat;
    }

    /// <summary>
    /// Menentukan status modul berdasarkan waktu acuan terhadap tanggal mulai dan tenggat.
    /// </summary>
    public static string HitungStatusModul(string tanggalMulai, string tenggat, string waktuAcuan)
    {
        string acuan = waktuAcuan.Trim();
        if (string.CompareOrdinal(acuan, tanggalMulai.Trim()) < 0)
            return ModulBelumDimulai;
        if (string.CompareOrdinal(acuan, tenggat.Trim()) <= 0)
            return ModulBerjalan;
        return ModulSelesai;
    }

    public static bool ApakahSudahJatuhTempo(string tenggat, string waktuAcuan)
    {
        return string.CompareOrdinal(tenggat.Trim(), waktuAcuan.Trim()) < 0;
    }

    /// <summary>
    /// BR-03: Menghitung persentase ketuntasan (jumlah modul jatuh tempo yang dikumpulkan / total modul jatuh tempo).
    /// Dibulatkan 2 angka di belakang koma (AwayFromZero agar konsisten dengan SQLite ROUND).
    /// </summary>
    public static double HitungKetuntasan(int modulJatuhTempoTerkumpul, int totalModulJatuhTempo)
    {
        if (totalModulJatuhTempo <= 0)
            return 100.0;

        double raw = (modulJatuhTempoTerkumpul * 100.0) / totalModulJatuhTempo;
        return Math.Round(raw, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// BR-04: Menghitung nilai akhir rata-rata berbobot.
    /// - Modul yang sudah dinilai masuk ke pembilang (Nilai * Bobot) dan penyebut (Bobot).
    /// - Modul yang sudah jatuh tempo tetapi Belum Mengumpulkan dihitung bernilai 0 (Bobot masuk ke penyebut).
    /// - Pengumpulan yang sudah masuk namun Belum Dinilai (Nilai == null) tidak masuk ke perhitungan.
    /// </summary>
    public static double HitungNilaiAkhirBerbobot(IEnumerable<ItemPerhitunganNilai> daftarItem)
    {
        double totalPoinBerbobot = 0.0;
        int totalBobotPembagi = 0;

        foreach (var item in daftarItem)
        {
            if (item.Nilai.HasValue)
            {
                totalPoinBerbobot += item.Nilai.Value * item.Bobot;
                totalBobotPembagi += item.Bobot;
            }
            else if (item.SudahJatuhTempo && !item.SudahMengumpulkan)
            {
                // Nilai 0 untuk modul jatuh tempo yang tidak dikumpulkan
                totalBobotPembagi += item.Bobot;
            }
        }

        if (totalBobotPembagi <= 0)
            return 0.0;

        return Math.Round(totalPoinBerbobot / totalBobotPembagi, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// BR-05: Mengevaluasi kriteria praktikan berisiko (R1, R2, R3).
    /// - R1: >= 2 modul jatuh tempo belum dikumpulkan.
    /// - R2: Ketuntasan di bawah ambang R2 (default 75%).
    /// - R3: Nilai akhir di bawah ambang R3 (default 60) dengan sedikitnya 2 modul sudah dinilai.
    /// </summary>
    public static HasilEvaluasiRisiko EvaluasiRisiko(
        int modulJatuhTempoBelumKumpul,
        double ketuntasan,
        double nilaiAkhir,
        int jumlahModulDinilai,
        double ambangKetuntasanR2 = 75.0,
        double ambangNilaiR3 = 60.0)
    {
        bool r1 = modulJatuhTempoBelumKumpul >= 2;
        bool r2 = ketuntasan < ambangKetuntasanR2;
        bool r3 = jumlahModulDinilai >= 2 && nilaiAkhir < ambangNilaiR3;

        var alasan = new List<string>();
        if (r1) alasan.Add("R1 (Belum Kumpul >= 2)");
        if (r2) alasan.Add("R2 (Ketuntasan < Ambang)");
        if (r3) alasan.Add("R3 (Nilai Akhir < Ambang)");

        return new HasilEvaluasiRisiko(
            R1: r1,
            R2: r2,
            R3: r3,
            IsBerisiko: r1 || r2 || r3,
            AlasanRisiko: string.Join("; ", alasan)
        );
    }

    /// <summary>
    /// Validasi NRP wajib tepat 10 digit angka.
    /// </summary>
    public static bool ValidasiNrp(string? nrp)
    {
        if (string.IsNullOrWhiteSpace(nrp) || nrp.Length != 10)
            return false;
        return nrp.All(char.IsAsciiDigit);
    }

    /// <summary>
    /// Validasi tautan tugas (FR-39): boleh kosong/null, tetapi jika diisi wajib URL HTTP atau HTTPS yang valid.
    /// </summary>
    public static bool ValidasiTautanTugas(string? tautan)
    {
        if (string.IsNullOrWhiteSpace(tautan))
            return true;

        string trimmed = tautan.Trim();
        if (!trimmed.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !trimmed.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return false;

        return Uri.TryCreate(trimmed, UriKind.Absolute, out var uri) &&
               (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
               !string.IsNullOrWhiteSpace(uri.Host) &&
               uri.Host.Contains('.');
    }

    /// <summary>
    /// Konversi input waktu format dd/MM/yyyy HH:mm atau yyyy-MM-dd HH:mm ke format standar database yyyy-MM-dd HH:mm.
    /// </summary>
    public static bool CobaFormatWaktuDatabase(string? input, out string hasilIso)
    {
        hasilIso = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        string[] formats = ["dd/MM/yyyy HH:mm", "yyyy-MM-dd HH:mm", "d/M/yyyy H:m", "yyyy-MM-dd"];
        if (DateTime.TryParseExact(input.Trim(), formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
        {
            hasilIso = dt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            return true;
        }
        return false;
    }

    public static string FormatWaktuTampilan(string? isoWaktu)
    {
        if (string.IsNullOrWhiteSpace(isoWaktu))
            return "-";
        if (DateTime.TryParseExact(isoWaktu.Trim(), ["yyyy-MM-dd HH:mm", "yyyy-MM-dd"], CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            return dt.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        return isoWaktu;
    }

    public static (string Level, int Skor) EvaluasiKekuatanPassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return ("Kosong", 0);
        int skor = 0;
        if (password.Length >= 6) skor++;
        if (password.Length >= 8) skor++;
        if (password.Any(char.IsUpper) && password.Any(char.IsLower)) skor++;
        if (password.Any(char.IsDigit)) skor++;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) skor++;

        return skor switch
        {
            <= 1 => ("Lemah", 1),
            2 => ("Cukup", 2),
            3 => ("Kuat", 3),
            _ => ("Sangat Kuat", 4)
        };
    }
}

public record ItemPerhitunganNilai(
    int Bobot,
    bool SudahJatuhTempo,
    bool SudahMengumpulkan,
    double? Nilai
);

public record HasilEvaluasiRisiko(
    bool R1,
    bool R2,
    bool R3,
    bool IsBerisiko,
    string AlasanRisiko
);
