using System.Globalization;
using System.Text;
using Simprak.Core.Models;

namespace Simprak.Core.Services;

public class UserSession
{
    public Pengguna? CurrentUser { get; private set; }
    public int? IdKelompokAsisten { get; private set; }
    public string? NamaKelompokAsisten { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;
    public bool IsAdmin => CurrentUser?.Peran == "Admin";
    public bool IsAsisten => CurrentUser?.Peran == "Asisten";

    public event Action? SessionChanged;

    public void SetSession(Pengguna user, int? idKelompok = null, string? namaKelompok = null)
    {
        CurrentUser = user;
        IdKelompokAsisten = idKelompok;
        NamaKelompokAsisten = namaKelompok;
        SessionChanged?.Invoke();
    }

    public void ClearSession()
    {
        CurrentUser = null;
        IdKelompokAsisten = null;
        NamaKelompokAsisten = null;
        SessionChanged?.Invoke();
    }
}

public static class CsvExporter
{
    /// <summary>
    /// Mengekspor data rekap praktikan ke string CSV sesuai pemisah daftar budaya (CultureInfo.TextInfo.ListSeparator)
    /// agar langsung terbaca rapi di Microsoft Excel maupun LibreOffice Calc.
    /// </summary>
    public static string ExportRekapPraktikan(IEnumerable<RekapPraktikanDto> items, CultureInfo? culture = null)
    {
        var ci = culture ?? CultureInfo.CurrentCulture;
        string sep = ci.TextInfo.ListSeparator;
        if (string.IsNullOrEmpty(sep)) sep = ",";

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(sep, [
            "NRP",
            "Nama Lengkap",
            "Angkatan",
            "Mata Kuliah",
            "Kelompok",
            "Ketuntasan (%)",
            "Nilai Akhir",
            "Modul Belum Kumpul",
            "Terlambat",
            "Status Risiko",
            "Alasan Risiko"
        ]));

        foreach (var r in items)
        {
            sb.AppendLine(string.Join(sep, [
                EscapeCsv(r.Nrp, sep),
                EscapeCsv(r.NamaLengkap, sep),
                r.Angkatan.ToString(ci),
                EscapeCsv(r.MataKuliah, sep),
                EscapeCsv(r.NamaKelompok, sep),
                r.Ketuntasan.ToString("0.00", ci),
                r.NilaiAkhir.ToString("0.00", ci),
                r.ModulBelumKumpul.ToString(ci),
                r.JumlahTerlambat.ToString(ci),
                EscapeCsv(r.StatusLabel, sep),
                EscapeCsv(r.AlasanRisiko, sep)
            ]));
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string? value, string sep)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;
        if (value.Contains(sep) || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}
