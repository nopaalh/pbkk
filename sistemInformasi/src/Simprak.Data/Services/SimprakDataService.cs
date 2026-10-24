using System.Globalization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Simprak.Core.Models;
using Simprak.Core.Rules;
using Simprak.Core.Services;

namespace Simprak.Data.Services;

public class SimprakDataService
{
    private readonly string _connectionString;
    private readonly SqliteConnection? _sharedOpenConnection;
    private readonly UserSession _session;

    public SimprakDataService(string connectionString, UserSession session)
    {
        _connectionString = connectionString;
        _session = session;
    }

    public SimprakDataService(SqliteConnection sharedOpenConnection, UserSession session)
    {
        _sharedOpenConnection = sharedOpenConnection;
        _connectionString = sharedOpenConnection.ConnectionString;
        _session = session;
    }

    private SimprakDbContext CreateDbContext()
    {
        var builder = new DbContextOptionsBuilder<SimprakDbContext>();
        if (_sharedOpenConnection != null)
        {
            builder.UseSqlite(_sharedOpenConnection);
        }
        else
        {
            builder.UseSqlite(_connectionString);
        }
        var ctx = new SimprakDbContext(builder.Options);
        ctx.Database.ExecuteSqlRaw("PRAGMA foreign_keys = ON;");
        return ctx;
    }

    private SqliteConnection GetOpenConnection(out bool ownsConnection)
    {
        if (_sharedOpenConnection != null)
        {
            ownsConnection = false;
            return _sharedOpenConnection;
        }
        ownsConnection = true;
        var conn = new SqliteConnection(_connectionString);
        conn.Open();
        using var pragma = conn.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return conn;
    }

    public string GetNowString()
    {
        using var ctx = CreateDbContext();
        var acuan = ctx.Pengaturan.AsNoTracking().FirstOrDefault(x => x.Kunci == "waktu_acuan")?.Nilai;
        return !string.IsNullOrWhiteSpace(acuan)
            ? acuan
            : DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
    }

    private void CatatLog(SimprakDbContext ctx, string aksi, string entitas, int? idEntitas, string keterangan)
    {
        ctx.LogAktivitas.Add(new LogAktivitas
        {
            IdPengguna = _session.CurrentUser?.IdPengguna,
            Aksi = aksi,
            Entitas = entitas,
            IdEntitas = idEntitas,
            Keterangan = keterangan,
            Waktu = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
        });
    }

    // ========================================================================
    // 1. AUTENTIKASI & SESI (FR-01 s.d. FR-05)
    // ========================================================================

    public (bool Sukses, string Pesan) Login(string? username, string? password)
    {
        using var ctx = CreateDbContext();
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            CatatLog(ctx, "LOGIN_GAGAL", "Pengguna", null, "Percobaan login gagal: input kosong.");
            ctx.SaveChanges();
            return (false, "Username atau password salah");
        }

        string cleanUser = username.Trim();
        var user = ctx.Pengguna.FirstOrDefault(u => u.Username.ToLower() == cleanUser.ToLower());

        if (user == null || user.StatusAktif == 0 || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            CatatLog(ctx, "LOGIN_GAGAL", "Pengguna", user?.IdPengguna, $"Percobaan login gagal untuk username '{cleanUser}'.");
            ctx.SaveChanges();
            return (false, "Username atau password salah");
        }

        user.LoginTerakhir = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        var kelompok = ctx.Kelompok.AsNoTracking().FirstOrDefault(k => k.IdAsisten == user.IdPengguna);
        _session.SetSession(user, kelompok?.IdKelompok, kelompok?.NamaKelompok);

        CatatLog(ctx, "LOGIN", "Pengguna", user.IdPengguna, $"Pengguna '{user.NamaLengkap}' ({user.Peran}) berhasil login.");
        ctx.SaveChanges();
        return (true, "Login berhasil");
    }

    public void Logout()
    {
        if (_session.CurrentUser != null)
        {
            using var ctx = CreateDbContext();
            CatatLog(ctx, "LOGOUT", "Pengguna", _session.CurrentUser.IdPengguna, $"Pengguna '{_session.CurrentUser.NamaLengkap}' keluar dari aplikasi.");
            ctx.SaveChanges();
        }
        _session.ClearSession();
    }

    // ========================================================================
    // 2. PEMBACAAN VIEW SQL & KALKULASI C# (UNTUK UI & UJI KONSISTENSI)
    // ========================================================================

    public (double AmbangR2, double AmbangR3, string NamaProgram, string WaktuAcuan) GetPengaturanSistem()
    {
        using var ctx = CreateDbContext();
        var dict = ctx.Pengaturan.AsNoTracking().ToDictionary(x => x.Kunci, x => x.Nilai);
        double r2 = dict.TryGetValue("ambang_ketuntasan_r2", out var vr2) && double.TryParse(vr2, CultureInfo.InvariantCulture, out var pr2) ? pr2 : 75.0;
        double r3 = dict.TryGetValue("ambang_nilai_r3", out var vr3) && double.TryParse(vr3, CultureInfo.InvariantCulture, out var pr3) ? pr3 : 60.0;
        string nama = dict.TryGetValue("nama_program", out var vnama) ? vnama : "SIMPRAK";
        string acuan = dict.TryGetValue("waktu_acuan", out var vacuan) ? vacuan : "2026-10-21 23:59";
        return (r2, r3, nama, acuan);
    }

    /// <summary>
    /// Membaca hasil langsung dari view SQLite v_rekap_praktikan dan v_praktikan_berisiko.
    /// </summary>
    public List<RekapPraktikanDto> GetRekapPraktikanFromSqlView(int? filterKelompok = null)
    {
        var conn = GetOpenConnection(out bool owns);
        try
        {
            var berisikoMap = new Dictionary<int, string>();
            using (var cmdRisk = conn.CreateCommand())
            {
                cmdRisk.CommandText = "SELECT id_praktikan, alasan_risiko FROM v_praktikan_berisiko;";
                using var rdr = cmdRisk.ExecuteReader();
                while (rdr.Read())
                {
                    berisikoMap[rdr.GetInt32(0)] = rdr.GetString(1);
                }
            }

            var list = new List<RekapPraktikanDto>();
            using var cmd = conn.CreateCommand();
            int? effectiveKelompok = _session.IsAsisten ? _session.IdKelompokAsisten : filterKelompok;

            if (effectiveKelompok.HasValue)
            {
                cmd.CommandText = "SELECT id_praktikan, nrp, nama_lengkap, angkatan, mata_kuliah, id_kelompok, nomor_kelompok, nama_kelompok, modul_jatuh_tempo, modul_jatuh_tempo_terkumpul, modul_belum_kumpul, jumlah_terlambat, jumlah_modul_dinilai, ketuntasan, nilai_akhir FROM v_rekap_praktikan WHERE id_kelompok = $kel ORDER BY nrp;";
                cmd.Parameters.AddWithValue("$kel", effectiveKelompok.Value);
            }
            else
            {
                cmd.CommandText = "SELECT id_praktikan, nrp, nama_lengkap, angkatan, mata_kuliah, id_kelompok, nomor_kelompok, nama_kelompok, modul_jatuh_tempo, modul_jatuh_tempo_terkumpul, modul_belum_kumpul, jumlah_terlambat, jumlah_modul_dinilai, ketuntasan, nilai_akhir FROM v_rekap_praktikan ORDER BY nrp;";
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(0);
                bool isRisk = berisikoMap.TryGetValue(id, out string? alasan);
                list.Add(new RekapPraktikanDto
                {
                    IdPraktikan = id,
                    Nrp = reader.GetString(1),
                    NamaLengkap = reader.GetString(2),
                    Angkatan = reader.GetInt32(3),
                    MataKuliah = reader.GetString(4),
                    IdKelompok = reader.GetInt32(5),
                    NomorKelompok = reader.GetInt32(6),
                    NamaKelompok = reader.GetString(7),
                    ModulJatuhTempo = reader.GetInt32(8),
                    ModulJatuhTempoTerkumpul = reader.GetInt32(9),
                    ModulBelumKumpul = reader.GetInt32(10),
                    JumlahTerlambat = reader.GetInt32(11),
                    JumlahModulDinilai = reader.GetInt32(12),
                    Ketuntasan = reader.GetDouble(13),
                    NilaiAkhir = reader.GetDouble(14),
                    IsBerisiko = isRisk,
                    AlasanRisiko = alasan ?? string.Empty
                });
            }
            return list;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    /// <summary>
    /// Menghitung rekap seluruh praktikan murni menggunakan SimprakRules di C# (BR-01 s.d. BR-05)
    /// untuk divalidasi kesamaannya dengan View SQLite.
    /// </summary>
    public List<RekapPraktikanDto> HitungRekapPraktikanDiCSharp(int? filterKelompok = null)
    {
        using var ctx = CreateDbContext();
        var (ambangR2, ambangR3, _, waktuAcuan) = GetPengaturanSistem();

        int? effectiveKelompok = _session.IsAsisten ? _session.IdKelompokAsisten : filterKelompok;

        var kelompokDict = ctx.Kelompok.AsNoTracking().ToDictionary(k => k.IdKelompok);
        var moduls = ctx.Modul.AsNoTracking().OrderBy(m => m.IdModul).ToList();
        var praktikanQuery = ctx.Praktikan.AsNoTracking().AsQueryable();
        if (effectiveKelompok.HasValue)
            praktikanQuery = praktikanQuery.Where(p => p.IdKelompok == effectiveKelompok.Value);

        var praktikans = praktikanQuery.OrderBy(p => p.Nrp).ToList();
        var pengumpulans = ctx.Pengumpulan.AsNoTracking().ToList()
            .ToDictionary(g => (g.IdPraktikan, g.IdModul));

        var results = new List<RekapPraktikanDto>();

        foreach (var p in praktikans)
        {
            int jatuhTempo = 0;
            int jatuhTempoTerkumpul = 0;
            int belumKumpul = 0;
            int terlambat = 0;
            int dinilai = 0;
            var itemsNilai = new List<ItemPerhitunganNilai>();

            foreach (var m in moduls)
            {
                bool isDue = SimprakRules.ApakahSudahJatuhTempo(m.Tenggat, waktuAcuan);
                pengumpulans.TryGetValue((p.IdPraktikan, m.IdModul), out var g);

                string status = SimprakRules.HitungStatusPengumpulan(g?.WaktuKumpul, m.Tenggat);
                bool sudahKumpul = status != SimprakRules.StatusBelumMengumpulkan;

                if (isDue)
                {
                    jatuhTempo++;
                    if (sudahKumpul) jatuhTempoTerkumpul++;
                    else belumKumpul++;
                }

                if (status == SimprakRules.StatusTerlambat)
                    terlambat++;

                if (g?.Nilai != null)
                    dinilai++;

                itemsNilai.Add(new ItemPerhitunganNilai(
                    Bobot: m.Bobot,
                    SudahJatuhTempo: isDue,
                    SudahMengumpulkan: sudahKumpul,
                    Nilai: g?.Nilai
                ));
            }

            double ketuntasan = SimprakRules.HitungKetuntasan(jatuhTempoTerkumpul, jatuhTempo);
            double nilaiAkhir = SimprakRules.HitungNilaiAkhirBerbobot(itemsNilai);
            var evaluasiRisiko = SimprakRules.EvaluasiRisiko(belumKumpul, ketuntasan, nilaiAkhir, dinilai, ambangR2, ambangR3);

            kelompokDict.TryGetValue(p.IdKelompok, out var kel);

            results.Add(new RekapPraktikanDto
            {
                IdPraktikan = p.IdPraktikan,
                Nrp = p.Nrp,
                NamaLengkap = p.NamaLengkap,
                Angkatan = p.Angkatan,
                MataKuliah = p.MataKuliah,
                IdKelompok = p.IdKelompok,
                NomorKelompok = kel?.NomorKelompok ?? 0,
                NamaKelompok = kel?.NamaKelompok ?? string.Empty,
                ModulJatuhTempo = jatuhTempo,
                ModulJatuhTempoTerkumpul = jatuhTempoTerkumpul,
                ModulBelumKumpul = belumKumpul,
                JumlahTerlambat = terlambat,
                JumlahModulDinilai = dinilai,
                Ketuntasan = ketuntasan,
                NilaiAkhir = nilaiAkhir,
                IsBerisiko = evaluasiRisiko.IsBerisiko,
                AlasanRisiko = evaluasiRisiko.AlasanRisiko
            });
        }

        return results;
    }

    public List<PraktikanBerisikoDto> GetPraktikanBerisiko()
    {
        var conn = GetOpenConnection(out bool owns);
        try
        {
            var list = new List<PraktikanBerisikoDto>();
            using var cmd = conn.CreateCommand();
            if (_session.IsAsisten && _session.IdKelompokAsisten.HasValue)
            {
                cmd.CommandText = "SELECT id_praktikan, nrp, nama_lengkap, angkatan, mata_kuliah, id_kelompok, nomor_kelompok, nama_kelompok, nama_asisten, modul_belum_kumpul, jumlah_terlambat, jumlah_modul_dinilai, ketuntasan, nilai_akhir, r1, r2, r3, alasan_risiko FROM v_praktikan_berisiko WHERE id_kelompok = $kel ORDER BY nilai_akhir ASC, nrp ASC;";
                cmd.Parameters.AddWithValue("$kel", _session.IdKelompokAsisten.Value);
            }
            else
            {
                cmd.CommandText = "SELECT id_praktikan, nrp, nama_lengkap, angkatan, mata_kuliah, id_kelompok, nomor_kelompok, nama_kelompok, nama_asisten, modul_belum_kumpul, jumlah_terlambat, jumlah_modul_dinilai, ketuntasan, nilai_akhir, r1, r2, r3, alasan_risiko FROM v_praktikan_berisiko ORDER BY nilai_akhir ASC, nrp ASC;";
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PraktikanBerisikoDto
                {
                    IdPraktikan = reader.GetInt32(0),
                    Nrp = reader.GetString(1),
                    NamaLengkap = reader.GetString(2),
                    Angkatan = reader.GetInt32(3),
                    MataKuliah = reader.GetString(4),
                    IdKelompok = reader.GetInt32(5),
                    NomorKelompok = reader.GetInt32(6),
                    NamaKelompok = reader.GetString(7),
                    NamaAsisten = reader.GetString(8),
                    ModulBelumKumpul = reader.GetInt32(9),
                    JumlahTerlambat = reader.GetInt32(10),
                    JumlahModulDinilai = reader.GetInt32(11),
                    Ketuntasan = reader.GetDouble(12),
                    NilaiAkhir = reader.GetDouble(13),
                    R1 = reader.GetInt32(14) == 1,
                    R2 = reader.GetInt32(15) == 1,
                    R3 = reader.GetInt32(16) == 1,
                    AlasanRisiko = reader.GetString(17)
                });
            }
            return list;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public List<RekapKelompokDto> GetRekapKelompok()
    {
        var conn = GetOpenConnection(out bool owns);
        try
        {
            var list = new List<RekapKelompokDto>();
            using var cmd = conn.CreateCommand();
            if (_session.IsAsisten && _session.IdKelompokAsisten.HasValue)
            {
                cmd.CommandText = "SELECT id_kelompok, nomor_kelompok, nama_kelompok, id_asisten, nama_asisten, jumlah_anggota, rata_ketuntasan, rata_nilai_akhir, jumlah_berisiko FROM v_rekap_kelompok WHERE id_kelompok = $kel ORDER BY nomor_kelompok;";
                cmd.Parameters.AddWithValue("$kel", _session.IdKelompokAsisten.Value);
            }
            else
            {
                cmd.CommandText = "SELECT id_kelompok, nomor_kelompok, nama_kelompok, id_asisten, nama_asisten, jumlah_anggota, rata_ketuntasan, rata_nilai_akhir, jumlah_berisiko FROM v_rekap_kelompok ORDER BY nomor_kelompok;";
            }

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new RekapKelompokDto
                {
                    IdKelompok = reader.GetInt32(0),
                    NomorKelompok = reader.GetInt32(1),
                    NamaKelompok = reader.GetString(2),
                    IdAsisten = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                    NamaAsisten = reader.GetString(4),
                    JumlahAnggota = reader.GetInt32(5),
                    RataKetuntasan = reader.GetDouble(6),
                    RataNilaiAkhir = reader.GetDouble(7),
                    JumlahBerisiko = reader.GetInt32(8)
                });
            }
            return list;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public List<ModulStatusDto> GetDaftarModulStatus()
    {
        var conn = GetOpenConnection(out bool owns);
        try
        {
            var list = new List<ModulStatusDto>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id_modul, kode_modul, judul_modul, kategori, tanggal_mulai, tenggat, bobot, status_modul, total_praktikan, jumlah_terkumpul, jumlah_dinilai FROM v_modul_status ORDER BY kode_modul;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new ModulStatusDto
                {
                    IdModul = reader.GetInt32(0),
                    KodeModul = reader.GetString(1),
                    JudulModul = reader.GetString(2),
                    Kategori = reader.GetString(3),
                    TanggalMulai = reader.GetString(4),
                    Tenggat = reader.GetString(5),
                    Bobot = reader.GetInt32(6),
                    StatusModul = reader.GetString(7),
                    TotalPraktikan = reader.GetInt32(8),
                    JumlahTerkumpul = reader.GetInt32(9),
                    JumlahDinilai = reader.GetInt32(10)
                });
            }
            return list;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    public DashboardRingkasanDto GetDashboardData()
    {
        var rekapPraktikan = GetRekapPraktikanFromSqlView();
        var rekapKelompok = GetRekapKelompok();
        var modulStatus = GetDaftarModulStatus();
        var berisiko = GetPraktikanBerisiko();

        var dto = new DashboardRingkasanDto
        {
            TotalPraktikan = rekapPraktikan.Count,
            TotalKelompok = rekapKelompok.Count,
            ModulAktif = modulStatus.Count(m => m.StatusModul == SimprakRules.ModulBerjalan),
            RataKetuntasanGlobal = rekapPraktikan.Count == 0 ? 0 : Math.Round(rekapPraktikan.Average(x => x.Ketuntasan), 2),
            RataNilaiGlobal = rekapPraktikan.Count == 0 ? 0 : Math.Round(rekapPraktikan.Average(x => x.NilaiAkhir), 2),
            TotalPraktikanBerisiko = berisiko.Count,
            DaftarBerisikoTeratas = berisiko.Take(6).ToList(),
            AktivitasTerbaru = GetLogAktivitas(limit: 8)
        };

        if (_session.IsAsisten && _session.IdKelompokAsisten.HasValue)
        {
            // Untuk Asisten: grafik menampilkan ketuntasan per modul di kelompoknya
            using var ctx = CreateDbContext();
            int kelId = _session.IdKelompokAsisten.Value;
            int anggotaCount = Math.Max(1, rekapPraktikan.Count);
            foreach (var m in modulStatus)
            {
                int kumpulKelompok = ctx.Pengumpulan.AsNoTracking()
                    .Count(g => g.IdModul == m.IdModul && g.WaktuKumpul != null && g.WaktuKumpul != "" &&
                                ctx.Praktikan.Any(p => p.IdPraktikan == g.IdPraktikan && p.IdKelompok == kelId));
                double pct = Math.Round(kumpulKelompok * 100.0 / anggotaCount, 1);
                dto.GrafikKetuntasan.Add(new ChartPointDto
                {
                    Label = m.KodeModul,
                    Value = pct,
                    IsWarning = pct < 75.0
                });
            }
        }
        else
        {
            // Untuk Admin: grafik ketuntasan per kelompok dengan garis ambang 75%
            foreach (var k in rekapKelompok)
            {
                dto.GrafikKetuntasan.Add(new ChartPointDto
                {
                    Label = $"K-{k.NomorKelompok:00}",
                    Value = k.RataKetuntasan,
                    IsWarning = k.RataKetuntasan < 75.0
                });
            }
        }

        // Distribusi nilai akhir dalam 5 rentang: <50, 50-59, 60-69, 70-84, 85-100
        dto.DistribusiNilai =
        [
            new ChartPointDto { Label = "< 50", Value = rekapPraktikan.Count(x => x.NilaiAkhir < 50), IsWarning = true },
            new ChartPointDto { Label = "50–59", Value = rekapPraktikan.Count(x => x.NilaiAkhir >= 50 && x.NilaiAkhir < 60), IsWarning = true },
            new ChartPointDto { Label = "60–69", Value = rekapPraktikan.Count(x => x.NilaiAkhir >= 60 && x.NilaiAkhir < 70) },
            new ChartPointDto { Label = "70–84", Value = rekapPraktikan.Count(x => x.NilaiAkhir >= 70 && x.NilaiAkhir < 85) },
            new ChartPointDto { Label = "85–100", Value = rekapPraktikan.Count(x => x.NilaiAkhir >= 85) }
        ];

        return dto;
    }

    public DetailPraktikanDto? GetDetailPraktikan(int idPraktikan)
    {
        var semuaRekap = GetRekapPraktikanFromSqlView();
        var profil = semuaRekap.FirstOrDefault(p => p.IdPraktikan == idPraktikan);
        if (profil == null)
            return null;

        if (_session.IsAsisten && _session.IdKelompokAsisten != profil.IdKelompok)
            throw new UnauthorizedAccessException("Asisten hanya dapat melihat detail praktikan di kelompok bimbingannya.");

        var kelompokInfo = GetRekapKelompok().FirstOrDefault(k => k.IdKelompok == profil.IdKelompok);

        var conn = GetOpenConnection(out bool owns);
        try
        {
            var riwayat = new List<StatusPengumpulanDto>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id_praktikan, nrp, nama_lengkap, id_kelompok, id_modul, kode_modul, judul_modul, kategori,
                       tanggal_mulai, tenggat, bobot, id_pengumpulan, waktu_kumpul, tautan_tugas, nilai, catatan,
                       id_penilai, nama_penilai, waktu_penilaian, status_pengumpulan, sudah_jatuh_tempo
                FROM v_status_pengumpulan
                WHERE id_praktikan = $pid
                ORDER BY kode_modul;";
            cmd.Parameters.AddWithValue("$pid", idPraktikan);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                riwayat.Add(new StatusPengumpulanDto
                {
                    IdPraktikan = reader.GetInt32(0),
                    Nrp = reader.GetString(1),
                    NamaLengkap = reader.GetString(2),
                    IdKelompok = reader.GetInt32(3),
                    IdModul = reader.GetInt32(4),
                    KodeModul = reader.GetString(5),
                    JudulModul = reader.GetString(6),
                    Kategori = reader.GetString(7),
                    TanggalMulai = reader.GetString(8),
                    Tenggat = reader.GetString(9),
                    Bobot = reader.GetInt32(10),
                    IdPengumpulan = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    WaktuKumpul = reader.IsDBNull(12) ? null : reader.GetString(12),
                    TautanTugas = reader.IsDBNull(13) ? null : reader.GetString(13),
                    Nilai = reader.IsDBNull(14) ? null : reader.GetDouble(14),
                    Catatan = reader.IsDBNull(15) ? null : reader.GetString(15),
                    IdPenilai = reader.IsDBNull(16) ? null : reader.GetInt32(16),
                    NamaPenilai = reader.IsDBNull(17) ? null : reader.GetString(17),
                    WaktuPenilaian = reader.IsDBNull(18) ? null : reader.GetString(18),
                    StatusPengumpulan = reader.GetString(19),
                    SudahJatuhTempo = reader.GetInt32(20) == 1
                });
            }

            // Hitung rata-rata kelompok per modul untuk grafik tren perbandingan
            var tren = new List<ChartPointDto>();
            foreach (var item in riwayat)
            {
                using var avgCmd = conn.CreateCommand();
                avgCmd.CommandText = "SELECT COALESCE(ROUND(AVG(nilai), 2), 0) FROM v_status_pengumpulan WHERE id_kelompok = $kel AND id_modul = $mod AND nilai IS NOT NULL;";
                avgCmd.Parameters.AddWithValue("$kel", profil.IdKelompok);
                avgCmd.Parameters.AddWithValue("$mod", item.IdModul);
                double avgKel = Convert.ToDouble(avgCmd.ExecuteScalar() ?? 0.0, CultureInfo.InvariantCulture);
                item.RataKelompokModul = avgKel;

                if (item.Nilai.HasValue || item.SudahJatuhTempo)
                {
                    tren.Add(new ChartPointDto
                    {
                        Label = item.KodeModul,
                        Value = item.Nilai ?? 0.0,
                        SecondaryValue = avgKel
                    });
                }
            }

            return new DetailPraktikanDto
            {
                Profil = profil,
                NamaAsisten = kelompokInfo?.NamaAsisten ?? "Belum Ditugaskan",
                RiwayatModul = riwayat,
                TrenNilaiVsKelompok = tren
            };
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    // ========================================================================
    // 3. MANAJEMEN PRAKTIKAN (FR-11 s.d. FR-16, BR-02)
    // ========================================================================

    public (bool Sukses, string Pesan) TambahAtauUbahPraktikan(Praktikan input)
    {
        if (_session.IsAsisten && _session.IdKelompokAsisten != input.IdKelompok)
            return (false, "Asisten hanya dapat mengelola praktikan pada kelompok yang dibimbingnya.");

        string nrp = input.Nrp?.Trim() ?? string.Empty;
        if (!SimprakRules.ValidasiNrp(nrp))
            return (false, "NRP harus terdiri atas tepat 10 digit angka.");

        if (string.IsNullOrWhiteSpace(input.NamaLengkap) || input.NamaLengkap.Trim().Length < 2)
            return (false, "Nama lengkap praktikan wajib diisi (minimal 2 karakter).");

        if (input.Angkatan < 2018 || input.Angkatan > 2030)
            return (false, "Tahun angkatan harus berada di antara 2018 dan 2030.");

        if (string.IsNullOrWhiteSpace(input.MataKuliah))
            return (false, "Mata kuliah wajib dipilih.");

        using var ctx = CreateDbContext();
        bool nrpDuplikat = ctx.Praktikan.Any(p => p.Nrp == nrp && p.IdPraktikan != input.IdPraktikan);
        if (nrpDuplikat)
            return (false, $"NRP {nrp} sudah terdaftar pada praktikan lain.");

        if (!ctx.Kelompok.Any(k => k.IdKelompok == input.IdKelompok))
            return (false, "Kelompok praktikum tidak valid.");

        if (input.IdPraktikan == 0)
        {
            var entity = new Praktikan
            {
                Nrp = nrp,
                NamaLengkap = input.NamaLengkap.Trim(),
                Angkatan = input.Angkatan,
                MataKuliah = input.MataKuliah.Trim(),
                IdKelompok = input.IdKelompok
            };
            ctx.Praktikan.Add(entity);
            ctx.SaveChanges();
            CatatLog(ctx, "TAMBAH", "Praktikan", entity.IdPraktikan, $"Menambahkan praktikan baru {entity.Nrp} - {entity.NamaLengkap}.");
            ctx.SaveChanges();
            return (true, "Data praktikan berhasil ditambahkan.");
        }
        else
        {
            var entity = ctx.Praktikan.FirstOrDefault(p => p.IdPraktikan == input.IdPraktikan);
            if (entity == null)
                return (false, "Data praktikan tidak ditemukan.");

            entity.Nrp = nrp;
            entity.NamaLengkap = input.NamaLengkap.Trim();
            entity.Angkatan = input.Angkatan;
            entity.MataKuliah = input.MataKuliah.Trim();
            entity.IdKelompok = input.IdKelompok;
            CatatLog(ctx, "UBAH", "Praktikan", entity.IdPraktikan, $"Memperbarui data praktikan {entity.Nrp} - {entity.NamaLengkap}.");
            ctx.SaveChanges();
            return (true, "Data praktikan berhasil diperbarui.");
        }
    }

    public (bool Sukses, string Pesan) HapusPraktikan(int idPraktikan)
    {
        using var ctx = CreateDbContext();
        var entity = ctx.Praktikan.FirstOrDefault(p => p.IdPraktikan == idPraktikan);
        if (entity == null)
            return (false, "Data praktikan tidak ditemukan.");

        if (_session.IsAsisten && _session.IdKelompokAsisten != entity.IdKelompok)
            return (false, "Asisten tidak memiliki akses untuk menghapus praktikan kelompok lain.");

        bool punyaPengumpulan = ctx.Pengumpulan.Any(g => g.IdPraktikan == idPraktikan);
        if (punyaPengumpulan)
            return (false, "Praktikan yang sudah memiliki riwayat pengumpulan/nilai tidak dapat dihapus.");

        string info = $"{entity.Nrp} - {entity.NamaLengkap}";
        ctx.Praktikan.Remove(entity);
        CatatLog(ctx, "HAPUS", "Praktikan", idPraktikan, $"Menghapus praktikan {info}.");
        ctx.SaveChanges();
        return (true, "Praktikan berhasil dihapus.");
    }

    // ========================================================================
    // 4. MANAJEMEN KELOMPOK PRAKTIKUM (FR-17 s.d. FR-20)
    // ========================================================================

    public List<Pengguna> GetAsistenTersediaUntukKelompok(int? idKelompokSedangDiedit = null)
    {
        using var ctx = CreateDbContext();
        int idKelompokDikecualikan = idKelompokSedangDiedit ?? 0;
        var dipakaiKelompokLain = ctx.Kelompok.AsNoTracking()
            .Where(k => k.IdAsisten != null && k.IdKelompok != idKelompokDikecualikan)
            .Select(k => k.IdAsisten!.Value)
            .ToHashSet();

        return ctx.Pengguna.AsNoTracking()
            .Where(u => u.Peran == "Asisten" && u.StatusAktif == 1 && !dipakaiKelompokLain.Contains(u.IdPengguna))
            .OrderBy(u => u.NamaLengkap)
            .ToList();
    }

    public (bool Sukses, string Pesan) TambahAtauUbahKelompok(Kelompok input)
    {
        if (!_session.IsAdmin)
            return (false, "Hanya Koordinator Lab (Admin) yang dapat mengelola kelompok praktikum.");

        if (input.NomorKelompok <= 0)
            return (false, "Nomor kelompok harus lebih besar dari 0.");

        if (string.IsNullOrWhiteSpace(input.NamaKelompok) || input.NamaKelompok.Trim().Length < 3)
            return (false, "Nama kelompok minimal 3 karakter.");

        using var ctx = CreateDbContext();
        if (ctx.Kelompok.Any(k => k.NomorKelompok == input.NomorKelompok && k.IdKelompok != input.IdKelompok))
            return (false, $"Nomor kelompok {input.NomorKelompok} sudah digunakan.");

        if (ctx.Kelompok.Any(k => k.NamaKelompok.ToLower() == input.NamaKelompok.Trim().ToLower() && k.IdKelompok != input.IdKelompok))
            return (false, $"Nama kelompok '{input.NamaKelompok.Trim()}' sudah digunakan.");

        if (input.IdAsisten.HasValue)
        {
            var asisten = ctx.Pengguna.FirstOrDefault(u => u.IdPengguna == input.IdAsisten.Value);
            if (asisten == null || asisten.Peran != "Asisten" || asisten.StatusAktif == 0)
                return (false, "Pembimbing harus merupakan akun dengan peran Asisten yang berstatus aktif.");

            if (ctx.Kelompok.Any(k => k.IdAsisten == input.IdAsisten.Value && k.IdKelompok != input.IdKelompok))
                return (false, "Asisten tersebut sudah membimbing kelompok praktikum lain.");
        }

        if (input.IdKelompok == 0)
        {
            var entity = new Kelompok
            {
                NomorKelompok = input.NomorKelompok,
                NamaKelompok = input.NamaKelompok.Trim(),
                IdAsisten = input.IdAsisten
            };
            ctx.Kelompok.Add(entity);
            ctx.SaveChanges();
            CatatLog(ctx, "TAMBAH", "Kelompok", entity.IdKelompok, $"Menambahkan kelompok #{entity.NomorKelompok} ({entity.NamaKelompok}).");
            ctx.SaveChanges();
            return (true, "Kelompok praktikum berhasil ditambahkan.");
        }
        else
        {
            var entity = ctx.Kelompok.FirstOrDefault(k => k.IdKelompok == input.IdKelompok);
            if (entity == null)
                return (false, "Kelompok tidak ditemukan.");

            entity.NomorKelompok = input.NomorKelompok;
            entity.NamaKelompok = input.NamaKelompok.Trim();
            entity.IdAsisten = input.IdAsisten;
            CatatLog(ctx, "UBAH", "Kelompok", entity.IdKelompok, $"Memperbarui kelompok #{entity.NomorKelompok} ({entity.NamaKelompok}).");
            ctx.SaveChanges();
            return (true, "Kelompok praktikum berhasil diperbarui.");
        }
    }

    public (bool Sukses, string Pesan) HapusKelompok(int idKelompok)
    {
        if (!_session.IsAdmin)
            return (false, "Hanya Admin yang dapat menghapus kelompok.");

        using var ctx = CreateDbContext();
        var entity = ctx.Kelompok.FirstOrDefault(k => k.IdKelompok == idKelompok);
        if (entity == null)
            return (false, "Kelompok tidak ditemukan.");

        if (ctx.Praktikan.Any(p => p.IdKelompok == idKelompok))
            return (false, "Kelompok yang masih memiliki anggota praktikan tidak dapat dihapus.");

        string nama = entity.NamaKelompok;
        ctx.Kelompok.Remove(entity);
        CatatLog(ctx, "HAPUS", "Kelompok", idKelompok, $"Menghapus kelompok '{nama}'.");
        ctx.SaveChanges();
        return (true, "Kelompok berhasil dihapus.");
    }

    // ========================================================================
    // 5. MANAJEMEN MODUL PENUGASAN (FR-21 s.d. FR-25)
    // ========================================================================

    public (bool Sukses, string Pesan) TambahAtauUbahModul(Modul input)
    {
        if (!_session.IsAdmin)
            return (false, "Hanya Admin yang dapat menambah atau mengubah modul penugasan.");

        if (string.IsNullOrWhiteSpace(input.KodeModul) || input.KodeModul.Trim().Length < 2)
            return (false, "Kode modul wajib diisi.");

        if (string.IsNullOrWhiteSpace(input.JudulModul) || input.JudulModul.Trim().Length < 3)
            return (false, "Judul modul wajib diisi (minimal 3 karakter).");

        if (input.Bobot < 1 || input.Bobot > 100)
            return (false, "Bobot modul harus bernilai antara 1 hingga 100.");

        if (!SimprakRules.CobaFormatWaktuDatabase(input.TanggalMulai, out string isoMulai))
            return (false, "Format tanggal mulai tidak valid (gunakan yyyy-MM-dd HH:mm atau dd/MM/yyyy HH:mm).");

        if (!SimprakRules.CobaFormatWaktuDatabase(input.Tenggat, out string isoTenggat))
            return (false, "Format tenggat tidak valid (gunakan yyyy-MM-dd HH:mm atau dd/MM/yyyy HH:mm).");

        if (string.CompareOrdinal(isoTenggat, isoMulai) < 0)
            return (false, "Tenggat modul tidak boleh lebih awal dari tanggal mulai.");

        using var ctx = CreateDbContext();
        string kode = input.KodeModul.Trim().ToUpperInvariant();
        if (ctx.Modul.Any(m => m.KodeModul.ToUpper() == kode && m.IdModul != input.IdModul))
            return (false, $"Kode modul '{kode}' sudah digunakan.");

        if (input.IdModul == 0)
        {
            var entity = new Modul
            {
                KodeModul = kode,
                JudulModul = input.JudulModul.Trim(),
                Kategori = input.Kategori,
                TanggalMulai = isoMulai,
                Tenggat = isoTenggat,
                Bobot = input.Bobot
            };
            ctx.Modul.Add(entity);
            ctx.SaveChanges();
            CatatLog(ctx, "TAMBAH", "Modul", entity.IdModul, $"Menambahkan modul {entity.KodeModul}: {entity.JudulModul}.");
            ctx.SaveChanges();
            return (true, "Modul berhasil ditambahkan.");
        }
        else
        {
            var entity = ctx.Modul.FirstOrDefault(m => m.IdModul == input.IdModul);
            if (entity == null)
                return (false, "Data modul tidak ditemukan.");

            entity.KodeModul = kode;
            entity.JudulModul = input.JudulModul.Trim();
            entity.Kategori = input.Kategori;
            entity.TanggalMulai = isoMulai;
            entity.Tenggat = isoTenggat;
            entity.Bobot = input.Bobot;
            CatatLog(ctx, "UBAH", "Modul", entity.IdModul, $"Memperbarui modul {entity.KodeModul}: {entity.JudulModul}.");
            ctx.SaveChanges();
            return (true, "Modul berhasil diperbarui.");
        }
    }

    public (bool Sukses, string Pesan) HapusModul(int idModul)
    {
        if (!_session.IsAdmin)
            return (false, "Hanya Admin yang dapat menghapus modul.");

        using var ctx = CreateDbContext();
        var entity = ctx.Modul.FirstOrDefault(m => m.IdModul == idModul);
        if (entity == null)
            return (false, "Modul tidak ditemukan.");

        string info = $"{entity.KodeModul} - {entity.JudulModul}";
        ctx.Modul.Remove(entity);
        CatatLog(ctx, "HAPUS", "Modul", idModul, $"Menghapus modul {info}.");
        ctx.SaveChanges();
        return (true, "Modul berhasil dihapus.");
    }

    // ========================================================================
    // 6. PENGUMPULAN, TAUTAN, & PENILAIAN BATCH (FR-26 s.d. FR-33, FR-39)
    // ========================================================================

    public List<StatusPengumpulanDto> GetTabelPenilaian(int idModul, int idKelompok)
    {
        if (_session.IsAsisten && _session.IdKelompokAsisten != idKelompok)
            throw new UnauthorizedAccessException("Asisten hanya dapat mengakses penilaian kelompok yang dibimbingnya.");

        var conn = GetOpenConnection(out bool owns);
        try
        {
            var list = new List<StatusPengumpulanDto>();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT id_praktikan, nrp, nama_lengkap, id_kelompok, id_modul, kode_modul, judul_modul, kategori,
                       tanggal_mulai, tenggat, bobot, id_pengumpulan, waktu_kumpul, tautan_tugas, nilai, catatan,
                       id_penilai, nama_penilai, waktu_penilaian, status_pengumpulan, sudah_jatuh_tempo
                FROM v_status_pengumpulan
                WHERE id_modul = $mod AND id_kelompok = $kel
                ORDER BY nrp;";
            cmd.Parameters.AddWithValue("$mod", idModul);
            cmd.Parameters.AddWithValue("$kel", idKelompok);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new StatusPengumpulanDto
                {
                    IdPraktikan = reader.GetInt32(0),
                    Nrp = reader.GetString(1),
                    NamaLengkap = reader.GetString(2),
                    IdKelompok = reader.GetInt32(3),
                    IdModul = reader.GetInt32(4),
                    KodeModul = reader.GetString(5),
                    JudulModul = reader.GetString(6),
                    Kategori = reader.GetString(7),
                    TanggalMulai = reader.GetString(8),
                    Tenggat = reader.GetString(9),
                    Bobot = reader.GetInt32(10),
                    IdPengumpulan = reader.IsDBNull(11) ? null : reader.GetInt32(11),
                    WaktuKumpul = reader.IsDBNull(12) ? null : reader.GetString(12),
                    TautanTugas = reader.IsDBNull(13) ? null : reader.GetString(13),
                    Nilai = reader.IsDBNull(14) ? null : reader.GetDouble(14),
                    Catatan = reader.IsDBNull(15) ? null : reader.GetString(15),
                    IdPenilai = reader.IsDBNull(16) ? null : reader.GetInt32(16),
                    NamaPenilai = reader.IsDBNull(17) ? null : reader.GetString(17),
                    WaktuPenilaian = reader.IsDBNull(18) ? null : reader.GetString(18),
                    StatusPengumpulan = reader.GetString(19),
                    SudahJatuhTempo = reader.GetInt32(20) == 1
                });
            }
            return list;
        }
        finally
        {
            if (owns) conn.Dispose();
        }
    }

    /// <summary>
    /// Menyimpan perubahan pengumpulan, tautan, dan nilai untuk banyak praktikan sekaligus dalam satu transaksi.
    /// Mencatat log audit perubahan nilai (nilai lama -> nilai baru).
    /// </summary>
    public (bool Sukses, string Pesan, int JumlahTersimpan) SimpanPenilaianBatch(IEnumerable<InputPenilaianBarisDto> daftarBaris)
    {
        if (!_session.IsLoggedIn || _session.CurrentUser == null)
            return (false, "Sesi pengguna tidak aktif.", 0);

        using var ctx = CreateDbContext();
        using var trx = ctx.Database.BeginTransaction();
        try
        {
            int countUpdated = 0;
            string sekarang = DateTime.Now.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

            foreach (var row in daftarBaris)
            {
                var praktikan = ctx.Praktikan.AsNoTracking().FirstOrDefault(p => p.IdPraktikan == row.IdPraktikan);
                if (praktikan == null)
                    return (false, $"Praktikan ID {row.IdPraktikan} tidak ditemukan.", 0);

                if (_session.IsAsisten && _session.IdKelompokAsisten != praktikan.IdKelompok)
                    return (false, "Akses ditolak: Anda hanya dapat menilai kelompok bimbingan Anda sendiri.", 0);

                var modul = ctx.Modul.AsNoTracking().FirstOrDefault(m => m.IdModul == row.IdModul);
                if (modul == null)
                    return (false, $"Modul ID {row.IdModul} tidak ditemukan.", 0);

                string? isoWaktuKumpul = null;
                if (!string.IsNullOrWhiteSpace(row.WaktuKumpul))
                {
                    if (!SimprakRules.CobaFormatWaktuDatabase(row.WaktuKumpul, out string parsedIso))
                        return (false, $"Format waktu pengumpulan NRP {praktikan.Nrp} tidak valid (gunakan dd/MM/yyyy HH:mm).", 0);
                    isoWaktuKumpul = parsedIso;
                }

                if (row.Nilai.HasValue && string.IsNullOrWhiteSpace(isoWaktuKumpul))
                {
                    return (false, $"NRP {praktikan.Nrp}: Nilai tidak dapat diisi sebelum waktu pengumpulan tercatat.", 0);
                }

                if (row.Nilai.HasValue && (row.Nilai.Value < 0 || row.Nilai.Value > 100))
                {
                    return (false, $"NRP {praktikan.Nrp}: Nilai harus berada pada rentang 0 hingga 100.", 0);
                }

                string? cleanTautan = string.IsNullOrWhiteSpace(row.TautanTugas) ? null : row.TautanTugas.Trim();
                if (!SimprakRules.ValidasiTautanTugas(cleanTautan))
                {
                    return (false, $"NRP {praktikan.Nrp}: Tautan tugas harus berupa URL valid berprotokol HTTP atau HTTPS.", 0);
                }

                var existing = ctx.Pengumpulan.FirstOrDefault(g => g.IdPraktikan == row.IdPraktikan && g.IdModul == row.IdModul);

                // Jika baris kosong seluruhnya dan belum ada di database, lewati
                if (existing == null && isoWaktuKumpul == null && cleanTautan == null && !row.Nilai.HasValue && string.IsNullOrWhiteSpace(row.Catatan))
                    continue;

                if (existing == null)
                {
                    var baru = new Pengumpulan
                    {
                        IdPraktikan = row.IdPraktikan,
                        IdModul = row.IdModul,
                        WaktuKumpul = isoWaktuKumpul,
                        TautanTugas = cleanTautan,
                        Nilai = row.Nilai,
                        Catatan = string.IsNullOrWhiteSpace(row.Catatan) ? null : row.Catatan.Trim(),
                        IdPenilai = row.Nilai.HasValue ? _session.CurrentUser.IdPengguna : null,
                        WaktuPenilaian = row.Nilai.HasValue ? sekarang : null
                    };
                    ctx.Pengumpulan.Add(baru);
                    ctx.SaveChanges();

                    if (row.Nilai.HasValue)
                    {
                        CatatLog(ctx, "NILAI", "Pengumpulan", baru.IdPengumpulan,
                            $"Memberi nilai baru {modul.KodeModul} sebesar {row.Nilai.Value:0.##} untuk NRP {praktikan.Nrp} ({praktikan.NamaLengkap}).");
                    }
                    else
                    {
                        CatatLog(ctx, "KUMPUL", "Pengumpulan", baru.IdPengumpulan,
                            $"Mencatat pengumpulan {modul.KodeModul} untuk NRP {praktikan.Nrp} ({praktikan.NamaLengkap}).");
                    }
                    countUpdated++;
                }
                else
                {
                    bool berubah = existing.WaktuKumpul != isoWaktuKumpul ||
                                   existing.TautanTugas != cleanTautan ||
                                   existing.Nilai != row.Nilai ||
                                   (existing.Catatan ?? "") != (row.Catatan?.Trim() ?? "");

                    if (!berubah)
                        continue;

                    double? nilaiLama = existing.Nilai;
                    existing.WaktuKumpul = isoWaktuKumpul;
                    existing.TautanTugas = cleanTautan;
                    existing.Nilai = row.Nilai;
                    existing.Catatan = string.IsNullOrWhiteSpace(row.Catatan) ? null : row.Catatan.Trim();

                    if (row.Nilai.HasValue)
                    {
                        existing.IdPenilai = _session.CurrentUser.IdPengguna;
                        existing.WaktuPenilaian = sekarang;
                    }
                    else
                    {
                        existing.IdPenilai = null;
                        existing.WaktuPenilaian = null;
                    }

                    if (nilaiLama != row.Nilai)
                    {
                        string strLama = nilaiLama.HasValue ? nilaiLama.Value.ToString("0.##", CultureInfo.InvariantCulture) : "Belum Dinilai";
                        string strBaru = row.Nilai.HasValue ? row.Nilai.Value.ToString("0.##", CultureInfo.InvariantCulture) : "Belum Dinilai";
                        CatatLog(ctx, "NILAI", "Pengumpulan", existing.IdPengumpulan,
                            $"Memperbarui nilai {modul.KodeModul} dari {strLama} menjadi {strBaru} untuk NRP {praktikan.Nrp} ({praktikan.NamaLengkap}).");
                    }
                    else
                    {
                        CatatLog(ctx, "UBAH", "Pengumpulan", existing.IdPengumpulan,
                            $"Memperbarui data pengumpulan/tautan {modul.KodeModul} untuk NRP {praktikan.Nrp}.");
                    }
                    countUpdated++;
                }
            }

            ctx.SaveChanges();
            trx.Commit();
            return (true, $"Berhasil menyimpan {countUpdated} perubahan penilaian.", countUpdated);
        }
        catch (Exception ex)
        {
            trx.Rollback();
            return (false, $"Gagal menyimpan penilaian: {ex.InnerException?.Message ?? ex.Message}", 0);
        }
    }

    // ========================================================================
    // 7. LOG AKTIVITAS, PENGGUNA, & PENGATURAN (FR-36 s.d. FR-38)
    // ========================================================================

    public List<LogAktivitasViewDto> GetLogAktivitas(
        string? keyword = null,
        int? idPengguna = null,
        string? aksi = null,
        int limit = 200)
    {
        using var ctx = CreateDbContext();
        var userMap = ctx.Pengguna.AsNoTracking().ToDictionary(u => u.IdPengguna, u => u.NamaLengkap);
        var query = ctx.LogAktivitas.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            string kw = keyword.Trim().ToLower();
            query = query.Where(l => l.Keterangan.ToLower().Contains(kw) || l.Entitas.ToLower().Contains(kw));
        }
        if (idPengguna.HasValue && idPengguna.Value > 0)
        {
            query = query.Where(l => l.IdPengguna == idPengguna.Value);
        }
        if (!string.IsNullOrWhiteSpace(aksi) && aksi != "Semua")
        {
            query = query.Where(l => l.Aksi == aksi);
        }

        return query.OrderByDescending(l => l.IdLog)
            .Take(limit)
            .ToList()
            .Select(l => new LogAktivitasViewDto
            {
                IdLog = l.IdLog,
                IdPengguna = l.IdPengguna,
                NamaPengguna = l.IdPengguna.HasValue && userMap.TryGetValue(l.IdPengguna.Value, out var nm) ? nm : "Sistem",
                Aksi = l.Aksi,
                Entitas = l.Entitas,
                IdEntitas = l.IdEntitas,
                Keterangan = l.Keterangan,
                Waktu = l.Waktu
            })
            .ToList();
    }

    public List<Pengguna> GetDaftarPengguna()
    {
        using var ctx = CreateDbContext();
        return ctx.Pengguna.AsNoTracking().OrderBy(u => u.IdPengguna).ToList();
    }

    public (bool Sukses, string Pesan) TambahAtauUbahPengguna(Pengguna input, string? passwordBaru = null)
    {
        if (!_session.IsAdmin)
            return (false, "Hanya Admin yang dapat mengelola akun pengguna.");

        if (string.IsNullOrWhiteSpace(input.Username) || input.Username.Trim().Length < 3)
            return (false, "Username minimal 3 karakter.");

        if (string.IsNullOrWhiteSpace(input.NamaLengkap) || input.NamaLengkap.Trim().Length < 2)
            return (false, "Nama lengkap wajib diisi.");

        using var ctx = CreateDbContext();
        string uname = input.Username.Trim();
        if (ctx.Pengguna.Any(u => u.Username.ToLower() == uname.ToLower() && u.IdPengguna != input.IdPengguna))
            return (false, $"Username '{uname}' sudah digunakan.");

        if (input.IdPengguna == 0)
        {
            if (string.IsNullOrWhiteSpace(passwordBaru) || passwordBaru.Length < 6)
                return (false, "Password akun baru minimal 6 karakter.");

            var baru = new Pengguna
            {
                Username = uname,
                NamaLengkap = input.NamaLengkap.Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordBaru),
                Peran = input.Peran,
                Tema = input.Tema,
                StatusAktif = input.StatusAktif
            };
            ctx.Pengguna.Add(baru);
            ctx.SaveChanges();
            CatatLog(ctx, "TAMBAH", "Pengguna", baru.IdPengguna, $"Menambahkan pengguna baru '{baru.Username}' ({baru.Peran}).");
            ctx.SaveChanges();
            return (true, "Pengguna baru berhasil ditambahkan.");
        }
        else
        {
            var existing = ctx.Pengguna.FirstOrDefault(u => u.IdPengguna == input.IdPengguna);
            if (existing == null)
                return (false, "Pengguna tidak ditemukan.");

            // BR-02: Admin tidak dapat menonaktifkan akun sendiri atau mengubah perannya sendiri
            if (_session.CurrentUser?.IdPengguna == existing.IdPengguna)
            {
                if (input.StatusAktif == 0)
                    return (false, "Admin tidak dapat menonaktifkan akunnya sendiri.");
                if (input.Peran != existing.Peran)
                    return (false, "Admin tidak dapat mengubah perannya sendiri.");
            }

            existing.Username = uname;
            existing.NamaLengkap = input.NamaLengkap.Trim();
            existing.Peran = input.Peran;
            existing.StatusAktif = input.StatusAktif;
            if (!string.IsNullOrWhiteSpace(passwordBaru))
            {
                if (passwordBaru.Length < 6)
                    return (false, "Password baru minimal 6 karakter.");
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordBaru);
            }

            // Jika akun dinonaktifkan atau diubah jadi Admin, lepaskan dari pembimbing kelompok
            if (existing.StatusAktif == 0 || existing.Peran != "Asisten")
            {
                var kel = ctx.Kelompok.FirstOrDefault(k => k.IdAsisten == existing.IdPengguna);
                if (kel != null) kel.IdAsisten = null;
            }

            CatatLog(ctx, "UBAH", "Pengguna", existing.IdPengguna, $"Memperbarui akun pengguna '{existing.Username}'.");
            ctx.SaveChanges();
            return (true, "Data pengguna berhasil diperbarui.");
        }
    }

    public (bool Sukses, string Pesan) SimpanAmbangRisiko(double ambangKetuntasanR2, double ambangNilaiR3, string? namaProgram = null)
    {
        if (!_session.IsAdmin)
            return (false, "Hanya Admin yang dapat mengubah ambang risiko.");

        if (ambangKetuntasanR2 < 10 || ambangKetuntasanR2 > 100)
            return (false, "Ambang ketuntasan R2 harus berada di antara 10% hingga 100%.");

        if (ambangNilaiR3 < 10 || ambangNilaiR3 > 100)
            return (false, "Ambang nilai akhir R3 harus berada di antara 10 hingga 100.");

        using var ctx = CreateDbContext();
        var r2 = ctx.Pengaturan.FirstOrDefault(x => x.Kunci == "ambang_ketuntasan_r2");
        if (r2 != null) r2.Nilai = ambangKetuntasanR2.ToString("0.##", CultureInfo.InvariantCulture);

        var r3 = ctx.Pengaturan.FirstOrDefault(x => x.Kunci == "ambang_nilai_r3");
        if (r3 != null) r3.Nilai = ambangNilaiR3.ToString("0.##", CultureInfo.InvariantCulture);

        if (!string.IsNullOrWhiteSpace(namaProgram))
        {
            var np = ctx.Pengaturan.FirstOrDefault(x => x.Kunci == "nama_program");
            if (np != null) np.Nilai = namaProgram.Trim();
        }

        CatatLog(ctx, "UBAH", "Pengaturan", null, $"Memperbarui ambang risiko R2={ambangKetuntasanR2:0.##}% dan R3={ambangNilaiR3:0.##}.");
        ctx.SaveChanges();
        return (true, "Pengaturan ambang risiko berhasil disimpan.");
    }

    public (bool Sukses, string Pesan) GantiPassword(string passwordLama, string passwordBaru)
    {
        if (_session.CurrentUser == null)
            return (false, "Sesi tidak aktif.");

        if (string.IsNullOrWhiteSpace(passwordBaru) || passwordBaru.Length < 6)
            return (false, "Password baru minimal 6 karakter.");

        using var ctx = CreateDbContext();
        var user = ctx.Pengguna.FirstOrDefault(u => u.IdPengguna == _session.CurrentUser.IdPengguna);
        if (user == null || !BCrypt.Net.BCrypt.Verify(passwordLama, user.PasswordHash))
            return (false, "Password saat ini tidak sesuai.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordBaru);
        CatatLog(ctx, "UBAH", "Pengguna", user.IdPengguna, "Pengguna memperbarui password akun.");
        ctx.SaveChanges();
        return (true, "Password berhasil diperbarui.");
    }

    public void SimpanTemaPengguna(string tema)
    {
        if (_session.CurrentUser == null) return;
        using var ctx = CreateDbContext();
        var user = ctx.Pengguna.FirstOrDefault(u => u.IdPengguna == _session.CurrentUser.IdPengguna);
        if (user != null)
        {
            user.Tema = tema;
            _session.CurrentUser.Tema = tema;
            ctx.SaveChanges();
        }
    }
}
