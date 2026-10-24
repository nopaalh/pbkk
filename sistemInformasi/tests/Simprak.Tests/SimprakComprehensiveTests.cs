using System.Globalization;
using Microsoft.Data.Sqlite;
using Simprak.Core.Models;
using Simprak.Core.Rules;
using Simprak.Core.Services;
using Simprak.Data;
using Simprak.Data.Services;
using Xunit;

namespace Simprak.Tests;

public class SimprakComprehensiveTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly UserSession _session;
    private readonly SimprakDataService _service;

    public SimprakComprehensiveTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:;Foreign Keys=True");
        _connection.Open();
        string script = File.ReadAllText(DatabaseInitializer.FindSqlScriptPath());
        DatabaseInitializer.InitializeOnConnection(_connection, script);
        _session = new UserSession();
        _service = new SimprakDataService(_connection, _session);
    }

    public void Dispose()
    {
        _connection.Dispose();
    }

    // ========================================================================
    // 1. ATURAN BISNIS BR-01: STATUS PENGUMPULAN & BATAS WAKTU TEPAT TENGGAT (7 TEST)
    // ========================================================================

    [Fact]
    public void BR01_01_WaktuKumpulSebelumTenggat_MenghasilkanTepatWaktu()
    {
        string status = SimprakRules.HitungStatusPengumpulan("2026-09-07 18:00", "2026-09-07 23:59");
        Assert.Equal(SimprakRules.StatusTepatWaktu, status);
    }

    [Fact]
    public void BR01_02_WaktuKumpulTepatPadaMenitTenggat_MenghasilkanTepatWaktu()
    {
        string status = SimprakRules.HitungStatusPengumpulan("2026-09-07 23:59", "2026-09-07 23:59");
        Assert.Equal(SimprakRules.StatusTepatWaktu, status);
    }

    [Fact]
    public void BR01_03_WaktuKumpulSatuMenitMelewatiTenggat_MenghasilkanTerlambat()
    {
        string status = SimprakRules.HitungStatusPengumpulan("2026-09-08 00:00", "2026-09-07 23:59");
        Assert.Equal(SimprakRules.StatusTerlambat, status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BR01_04_06_WaktuKumpulKosongAtauNull_MenghasilkanBelumMengumpulkan(string? input)
    {
        string status = SimprakRules.HitungStatusPengumpulan(input, "2026-09-07 23:59");
        Assert.Equal(SimprakRules.StatusBelumMengumpulkan, status);
    }

    [Theory]
    [InlineData("2026-09-01 08:00", "2026-09-07 23:59", "2026-08-31 12:00", "Belum Dimulai")]
    [InlineData("2026-09-01 08:00", "2026-09-07 23:59", "2026-09-05 12:00", "Berjalan")]
    [InlineData("2026-09-01 08:00", "2026-09-07 23:59", "2026-09-08 00:01", "Selesai")]
    public void BR01_07_09_StatusModulBerdasarkanWaktuAcuan_SesuaiHarapan(string mulai, string tenggat, string acuan, string expected)
    {
        Assert.Equal(expected, SimprakRules.HitungStatusModul(mulai, tenggat, acuan));
    }

    // ========================================================================
    // 2. ATURAN BISNIS BR-03 & BR-04: KETUNTASAN & NILAI AKHIR BERBOBOT (7 TEST)
    // ========================================================================

    [Fact]
    public void BR03_10_SemuaModulJatuhTempoDikumpulkan_Ketuntasan100Persen()
    {
        Assert.Equal(100.0, SimprakRules.HitungKetuntasan(5, 5));
    }

    [Fact]
    public void BR03_11_TigaDariLimaModulDikumpulkan_Ketuntasan60Persen()
    {
        Assert.Equal(60.0, SimprakRules.HitungKetuntasan(3, 5));
    }

    [Fact]
    public void BR03_12_BelumAdaModulJatuhTempo_Ketuntasan100Persen()
    {
        Assert.Equal(100.0, SimprakRules.HitungKetuntasan(0, 0));
    }

    [Fact]
    public void BR03_13_PembulatanDuaDesimal_SesuaiAturan()
    {
        // 2 dari 3 = 66.67%
        Assert.Equal(66.67, SimprakRules.HitungKetuntasan(2, 3));
    }

    [Fact]
    public void BR04_14_NilaiAkhirBerbobot_MenghitungModulDinilaiDenganBenar()
    {
        var items = new[]
        {
            new ItemPerhitunganNilai(Bobot: 10, SudahJatuhTempo: true, SudahMengumpulkan: true, Nilai: 80),
            new ItemPerhitunganNilai(Bobot: 20, SudahJatuhTempo: true, SudahMengumpulkan: true, Nilai: 95)
        };
        // (80*10 + 95*20) / 30 = (800 + 1900) / 30 = 90.0
        Assert.Equal(90.0, SimprakRules.HitungNilaiAkhirBerbobot(items));
    }

    [Fact]
    public void BR04_15_ModulJatuhTempoBelumDikumpulkan_DihitungBernilaiNol()
    {
        var items = new[]
        {
            new ItemPerhitunganNilai(Bobot: 15, SudahJatuhTempo: true, SudahMengumpulkan: true, Nilai: 90),
            new ItemPerhitunganNilai(Bobot: 15, SudahJatuhTempo: true, SudahMengumpulkan: false, Nilai: null)
        };
        // (90*15 + 0*15) / 30 = 45.0
        Assert.Equal(45.0, SimprakRules.HitungNilaiAkhirBerbobot(items));
    }

    [Fact]
    public void BR04_16_PengumpulanSudahMasukTetapiBelumDinilai_TidakMasukPembagi()
    {
        var items = new[]
        {
            new ItemPerhitunganNilai(Bobot: 15, SudahJatuhTempo: true, SudahMengumpulkan: true, Nilai: 84),
            new ItemPerhitunganNilai(Bobot: 10, SudahJatuhTempo: false, SudahMengumpulkan: true, Nilai: null), // Sudah kumpul tapi belum dinilai
            new ItemPerhitunganNilai(Bobot: 10, SudahJatuhTempo: false, SudahMengumpulkan: false, Nilai: null) // Belum jatuh tempo
        };
        Assert.Equal(84.0, SimprakRules.HitungNilaiAkhirBerbobot(items));
    }

    // ========================================================================
    // 3. ATURAN BISNIS BR-05: DETEKSI PRAKTIKAN BERISIKO R1, R2, R3 (6 TEST)
    // ========================================================================

    [Fact]
    public void BR05_17_DuaModulJatuhTempoBelumKumpul_MemicuR1()
    {
        var res = SimprakRules.EvaluasiRisiko(modulJatuhTempoBelumKumpul: 2, ketuntasan: 80.0, nilaiAkhir: 80.0, jumlahModulDinilai: 3);
        Assert.True(res.R1);
        Assert.False(res.R2);
        Assert.False(res.R3);
        Assert.True(res.IsBerisiko);
        Assert.Contains("R1", res.AlasanRisiko);
    }

    [Fact]
    public void BR05_18_KetuntasanDiBawahAmbang75_MemicuR2()
    {
        var res = SimprakRules.EvaluasiRisiko(modulJatuhTempoBelumKumpul: 1, ketuntasan: 74.99, nilaiAkhir: 80.0, jumlahModulDinilai: 4, ambangKetuntasanR2: 75.0);
        Assert.False(res.R1);
        Assert.True(res.R2);
        Assert.True(res.IsBerisiko);
        Assert.Contains("R2", res.AlasanRisiko);
    }

    [Fact]
    public void BR05_19_NilaiAkhirDiBawahAmbangDenganMinimalDuaModulDinilai_MemicuR3()
    {
        var res = SimprakRules.EvaluasiRisiko(modulJatuhTempoBelumKumpul: 0, ketuntasan: 100.0, nilaiAkhir: 58.5, jumlahModulDinilai: 2, ambangKetuntasanR2: 75.0, ambangNilaiR3: 60.0);
        Assert.False(res.R1);
        Assert.False(res.R2);
        Assert.True(res.R3);
        Assert.True(res.IsBerisiko);
    }

    [Fact]
    public void BR05_20_NilaiAkhirDiBawahAmbangTetapiBaruSatuModulDinilai_TidakMemicuR3()
    {
        var res = SimprakRules.EvaluasiRisiko(modulJatuhTempoBelumKumpul: 0, ketuntasan: 100.0, nilaiAkhir: 45.0, jumlahModulDinilai: 1, ambangKetuntasanR2: 75.0, ambangNilaiR3: 60.0);
        Assert.False(res.R3);
        Assert.False(res.IsBerisiko);
    }

    [Fact]
    public void BR05_21_MemenuhiKetigaKriteriaSekaligus_MenghasilkanGabunganAlasanR1R2R3()
    {
        var res = SimprakRules.EvaluasiRisiko(modulJatuhTempoBelumKumpul: 2, ketuntasan: 60.0, nilaiAkhir: 50.0, jumlahModulDinilai: 3);
        Assert.True(res.R1 && res.R2 && res.R3);
        Assert.Equal("R1 (Belum Kumpul >= 2); R2 (Ketuntasan < Ambang); R3 (Nilai Akhir < Ambang)", res.AlasanRisiko);
    }

    [Fact]
    public void BR05_22_PerubahanAmbangRisikoOlehAdmin_MengubahJumlahPraktikanBerisikoSecaraDinamis()
    {
        _service.Login("admin", "admin123");
        int awal = _service.GetPraktikanBerisiko().Count;
        _service.SimpanAmbangRisiko(ambangKetuntasanR2: 95.0, ambangNilaiR3: 85.0);
        int setelahNaik = _service.GetPraktikanBerisiko().Count;
        Assert.True(setelahNaik > awal);
    }

    // ========================================================================
    // 4. KONSISTENSI PERHITUNGAN C# VS VIEW SQLITE UNTUK DATA CONTOH (4 TEST)
    // ========================================================================

    [Fact]
    public void SQL_23_PerhitunganKetuntasanDanNilaiAkhirCSharp_Identik100PersenDenganViewSqliteUntuk124Praktikan()
    {
        var dariSql = _service.GetRekapPraktikanFromSqlView();
        var dariCSharp = _service.HitungRekapPraktikanDiCSharp();

        Assert.Equal(124, dariSql.Count);
        Assert.Equal(dariSql.Count, dariCSharp.Count);

        for (int i = 0; i < dariSql.Count; i++)
        {
            Assert.Equal(dariSql[i].IdPraktikan, dariCSharp[i].IdPraktikan);
            Assert.Equal(dariSql[i].Ketuntasan, dariCSharp[i].Ketuntasan);
            Assert.Equal(dariSql[i].NilaiAkhir, dariCSharp[i].NilaiAkhir);
            Assert.Equal(dariSql[i].IsBerisiko, dariCSharp[i].IsBerisiko);
            Assert.Equal(dariSql[i].AlasanRisiko, dariCSharp[i].AlasanRisiko);
        }
    }

    [Fact]
    public void SQL_24_ViewRekapKelompok_SesuaiDengan12KelompokSeed()
    {
        var kelompok = _service.GetRekapKelompok();
        Assert.Equal(12, kelompok.Count);
        Assert.Equal(124, kelompok.Sum(k => k.JumlahAnggota));
    }

    [Fact]
    public void SQL_25_ViewModulStatus_MengelompokkanSelesaiBerjalanBelumDimulaiDenganBenar()
    {
        var moduls = _service.GetDaftarModulStatus();
        Assert.Equal(8, moduls.Count);
        Assert.Equal(5, moduls.Count(m => m.StatusModul == "Selesai"));
        Assert.Equal(2, moduls.Count(m => m.StatusModul == "Berjalan"));
        Assert.Equal(1, moduls.Count(m => m.StatusModul == "Belum Dimulai"));
    }

    [Fact]
    public void SQL_26_ViewDashboard_MenghasilkanMetrikAgregatValid()
    {
        var dash = _service.GetDashboardData();
        Assert.Equal(124, dash.TotalPraktikan);
        Assert.Equal(12, dash.TotalKelompok);
        Assert.Equal(2, dash.ModulAktif);
        Assert.True(dash.TotalPraktikanBerisiko > 0);
        Assert.Equal(5, dash.DistribusiNilai.Count);
        Assert.Equal(124, dash.DistribusiNilai.Sum(d => (int)d.Value));
    }

    // ========================================================================
    // 5. AUTENTIKASI, HAK AKSES ASISTEN, DAN AUDIT LOG (6 TEST)
    // ========================================================================

    [Fact]
    public void AUTH_27_LoginAdminDanAsisten_BerhasilDanMengaturSesi()
    {
        var resAdmin = _service.Login("admin", "admin123");
        Assert.True(resAdmin.Sukses);
        Assert.True(_session.IsAdmin);

        var resAsisten = _service.Login("asisten01", "asisten123");
        Assert.True(resAsisten.Sukses);
        Assert.True(_session.IsAsisten);
        Assert.Equal(1, _session.IdKelompokAsisten);
    }

    [Theory]
    [InlineData("admin", "salahpassword")]
    [InlineData("tidakada", "admin123")]
    [InlineData("", "")]
    public void AUTH_28_30_LoginGagal_MenampilkanPesanUmumDanMencatatLog(string user, string pass)
    {
        int logAwal = _service.GetLogAktivitas().Count;
        var res = _service.Login(user, pass);
        Assert.False(res.Sukses);
        Assert.Equal("Username atau password salah", res.Pesan);
        Assert.True(_service.GetLogAktivitas().Count > logAwal);
    }

    [Fact]
    public void AUTH_31_AsistenHanyaDapatMelihatDanMenilaiKelompokBimbingannya()
    {
        _service.Login("asisten01", "asisten123"); // Kelompok 1
        var daftarPraktikan = _service.GetRekapPraktikanFromSqlView();
        Assert.All(daftarPraktikan, p => Assert.Equal(1, p.IdKelompok));

        // Mencoba membuka tabel penilaian kelompok 2 harus melempar UnauthorizedAccessException
        Assert.Throws<UnauthorizedAccessException>(() => _service.GetTabelPenilaian(idModul: 1, idKelompok: 2));
    }

    [Fact]
    public void AUTH_32_AdminTidakDapatMenonaktifkanAkunSendiriAtauMengubahPerannyaSendiri()
    {
        _service.Login("admin", "admin123");
        var adminUser = _service.GetDaftarPengguna().First(u => u.Username == "admin");

        var cobaNonaktif = _service.TambahAtauUbahPengguna(new Pengguna
        {
            IdPengguna = adminUser.IdPengguna,
            Username = adminUser.Username,
            NamaLengkap = adminUser.NamaLengkap,
            Peran = "Admin",
            StatusAktif = 0
        });
        Assert.False(cobaNonaktif.Sukses);

        var cobaTurunPeran = _service.TambahAtauUbahPengguna(new Pengguna
        {
            IdPengguna = adminUser.IdPengguna,
            Username = adminUser.Username,
            NamaLengkap = adminUser.NamaLengkap,
            Peran = "Asisten",
            StatusAktif = 1
        });
        Assert.False(cobaTurunPeran.Sukses);
    }

    // ========================================================================
    // 6. PENILAIAN, AUDIT PERUBAHAN NILAI, & TAUTAN TUGAS FR-39 (7 TEST)
    // ========================================================================

    [Fact]
    public void GRADE_33_MenyimpanNilaiTanpaWaktuPengumpulan_DitolakOlehLayanan()
    {
        _service.Login("asisten01", "asisten123");
        var hasil = _service.SimpanPenilaianBatch([
            new InputPenilaianBarisDto
            {
                IdPraktikan = 1,
                IdModul = 8,
                WaktuKumpul = null,
                Nilai = 88
            }
        ]);
        Assert.False(hasil.Sukses);
        Assert.Contains("sebelum waktu pengumpulan tercatat", hasil.Pesan);
    }

    [Fact]
    public void GRADE_34_PerubahanNilai_MencatatNilaiLamaDanNilaiBaruDiLogAktivitas()
    {
        _service.Login("asisten01", "asisten123");
        var hasil = _service.SimpanPenilaianBatch([
            new InputPenilaianBarisDto
            {
                IdPraktikan = 1,
                IdModul = 1,
                WaktuKumpul = "06/09/2026 18:45",
                TautanTugas = "https://github.com/simprak-lab/5025241001-mod01",
                Nilai = 99,
                Catatan = "Sangat memuaskan"
            }
        ]);
        Assert.True(hasil.Sukses);

        var logTerbaru = _service.GetLogAktivitas(aksi: "NILAI", limit: 5).First();
        Assert.Contains("menjadi 99", logTerbaru.Keterangan);
        Assert.Contains("5025241001", logTerbaru.Keterangan);
    }

    [Theory]
    [InlineData("https://github.com/nopalh/simprak", true)]
    [InlineData("http://drive.google.com/file/d/123", true)]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("ftp://fileserver/tugas.zip", false)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("bukan-url-sama-sekali", false)]
    public void LINK_35_41_ValidasiMenyimpanDanMenghapusTautanTugas_SesuaiFR39(string? tautan, bool expectedValid)
    {
        Assert.Equal(expectedValid, SimprakRules.ValidasiTautanTugas(tautan));
    }

    [Fact]
    public void LINK_42_SimpanDanHapusTautanTugasPadaDatabase_BerfungsiPenuh()
    {
        _service.Login("asisten01", "asisten123");
        // Simpan tautan baru
        var simpan = _service.SimpanPenilaianBatch([
            new InputPenilaianBarisDto
            {
                IdPraktikan = 1,
                IdModul = 7,
                WaktuKumpul = "20/10/2026 15:00",
                TautanTugas = "https://docs.google.com/document/d/simprak-mod07",
                Nilai = 91
            }
        ]);
        Assert.True(simpan.Sukses);

        var detail = _service.GetDetailPraktikan(1)!;
        var mod7 = detail.RiwayatModul.First(m => m.IdModul == 7);
        Assert.Equal("https://docs.google.com/document/d/simprak-mod07", mod7.TautanTugas);

        // Hapus tautan (dikosongkan)
        var hapusTautan = _service.SimpanPenilaianBatch([
            new InputPenilaianBarisDto
            {
                IdPraktikan = 1,
                IdModul = 7,
                WaktuKumpul = "20/10/2026 15:00",
                TautanTugas = "",
                Nilai = 91
            }
        ]);
        Assert.True(hapusTautan.Sukses);
        Assert.Null(_service.GetDetailPraktikan(1)!.RiwayatModul.First(m => m.IdModul == 7).TautanTugas);
    }

    // ========================================================================
    // 7. VALIDASI MASTER DATA, PROTEKSI HAPUS (BR-02), MIGRASI SKEMA, & CSV (6 TEST)
    // ========================================================================

    [Fact]
    public void MASTER_43_ValidasiNrpGandaDanPenolakanHapusPraktikanBerpengumpulan()
    {
        _service.Login("admin", "admin123");
        // NRP ganda
        var dup = _service.TambahAtauUbahPraktikan(new Praktikan
        {
            Nrp = "5025241001",
            NamaLengkap = "Duplikat NRP",
            Angkatan = 2024,
            MataKuliah = "Grafika Komputer",
            IdKelompok = 1
        });
        Assert.False(dup.Sukses);

        // Tolak hapus praktikan ID 1 karena sudah punya riwayat pengumpulan
        var tolakHapus = _service.HapusPraktikan(1);
        Assert.False(tolakHapus.Sukses);

        // Tambah praktikan baru tanpa pengumpulan -> boleh dihapus
        var tambahBaru = _service.TambahAtauUbahPraktikan(new Praktikan
        {
            Nrp = "5025259999",
            NamaLengkap = "Praktikan Baru Uji",
            Angkatan = 2025,
            MataKuliah = "Grafika Komputer",
            IdKelompok = 1
        });
        Assert.True(tambahBaru.Sukses);
        var idBaru = _service.GetRekapPraktikanFromSqlView().First(p => p.Nrp == "5025259999").IdPraktikan;
        var hapusBaru = _service.HapusPraktikan(idBaru);
        Assert.True(hapusBaru.Sukses);
    }

    [Fact]
    public void MASTER_44_PenolakanHapusKelompokBeranggotaDanValidasiTanggalModul()
    {
        _service.Login("admin", "admin123");
        // Kelompok 1 punya anggota -> tidak boleh dihapus
        var hapusKel = _service.HapusKelompok(1);
        Assert.False(hapusKel.Sukses);

        // Modul dengan tenggat lebih awal dari tanggal mulai -> ditolak
        var modulSalah = _service.TambahAtauUbahModul(new Modul
        {
            KodeModul = "MOD-99",
            JudulModul = "Modul Tanggal Terbalik",
            Kategori = "Modul Mingguan",
            TanggalMulai = "2026-11-10 08:00",
            Tenggat = "2026-11-05 23:59",
            Bobot = 10
        });
        Assert.False(modulSalah.Sukses);
    }

    [Fact]
    public void SCHEMA_45_PeningkatanSkemaDatabaseLama_MenambahkanKolomTautanTanpaMenghapusData()
    {
        using var legacyConn = new SqliteConnection("Data Source=:memory:");
        legacyConn.Open();
        using (var cmd = legacyConn.CreateCommand())
        {
            cmd.CommandText = @"
                CREATE TABLE pengumpulan (
                    id_pengumpulan INTEGER PRIMARY KEY AUTOINCREMENT,
                    id_praktikan INTEGER NOT NULL,
                    id_modul INTEGER NOT NULL,
                    waktu_kumpul TEXT NULL,
                    nilai REAL NULL
                );
                INSERT INTO pengumpulan (id_praktikan, id_modul, waktu_kumpul, nilai) VALUES (1, 1, '2026-09-05 10:00', 88);";
            cmd.ExecuteNonQuery();
        }

        DatabaseInitializer.UpgradeSchemaIfNeeded(legacyConn);

        using var verifyCmd = legacyConn.CreateCommand();
        verifyCmd.CommandText = "SELECT nilai, tautan_tugas FROM pengumpulan WHERE id_praktikan = 1;";
        using var rdr = verifyCmd.ExecuteReader();
        Assert.True(rdr.Read());
        Assert.Equal(88.0, rdr.GetDouble(0));
        Assert.True(rdr.IsDBNull(1));
    }

    [Fact]
    public void CSV_46_EksporCsvMengikutiPemisahRegional_KomaDanTitikKoma()
    {
        var data = _service.GetRekapPraktikanFromSqlView().Take(3).ToList();

        var usCulture = new CultureInfo("en-US"); // Pemisah ','
        string csvUs = CsvExporter.ExportRekapPraktikan(data, usCulture);
        Assert.Contains("NRP,Nama Lengkap,Angkatan,Mata Kuliah", csvUs);

        var idCulture = new CultureInfo("id-ID"); // Pemisah ';'
        string csvId = CsvExporter.ExportRekapPraktikan(data, idCulture);
        Assert.Contains("NRP;Nama Lengkap;Angkatan;Mata Kuliah", csvId);
    }
}
