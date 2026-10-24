using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Simprak.App.Services;
using Simprak.Core.Models;
using Simprak.Core.Rules;
using Simprak.Core.Services;
using Simprak.Data;
using Simprak.Data.Services;

namespace Simprak.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly SimprakDataService _dataService;
    private readonly UserSession _session;
    private readonly string _dbPath;

    // Status Sesi & Navigasi
    [ObservableProperty] private bool _isLoggedIn;
    [ObservableProperty] private bool _isAdmin;
    [ObservableProperty] private string _namaPenggunaAktif = string.Empty;
    [ObservableProperty] private string _peranPenggunaAktif = string.Empty;
    [ObservableProperty] private string _namaProgramHeader = "SIMPRAK — Sistem Informasi Monitoring dan Penilaian Praktikum";
    [ObservableProperty] private string _activePage = "Dashboard";
    [ObservableProperty] private string _pageTitle = "Dashboard Analitik Praktikum";
    [ObservableProperty] private string _breadcrumb = "Beranda / Dashboard";
    [ObservableProperty] private string _temaAktif = "Terang";
    [ObservableProperty] private int _jumlahPraktikanBerisikoBadge;

    // Toast Notification
    private readonly DispatcherTimer _toastTimer = new() { Interval = TimeSpan.FromSeconds(2) };
    [ObservableProperty] private string _toastMessage = string.Empty;
    [ObservableProperty] private bool _isToastVisible;
    [ObservableProperty] private bool _isToastError;

    // Login Form
    [ObservableProperty] private string _loginUsername = "admin";
    [ObservableProperty] private string _loginPassword = string.Empty;
    [ObservableProperty] private bool _loginRememberMe = true;
    [ObservableProperty] private string _loginErrorMessage = string.Empty;

    // Pencarian Global (Ctrl+K)
    [ObservableProperty] private string _globalSearchQuery = string.Empty;

    // 1. Dashboard Data
    [ObservableProperty] private DashboardRingkasanDto _dashboard = new();
    [ObservableProperty] private string _judulGrafikKetuntasan = "Ketuntasan Kelompok Praktikum (Ambang 75%)";

    // 2. Halaman Praktikan & Filter
    private List<RekapPraktikanDto> _allPraktikan = new();
    public ObservableCollection<RekapPraktikanDto> FilteredPraktikan { get; } = new();
    public ObservableCollection<string> PilihanFilterKelompok { get; } = ["Semua Kelompok"];
    public ObservableCollection<string> PilihanFilterMatkul { get; } = ["Semua Mata Kuliah", "Grafika Komputer", "Struktur Data", "Basis Data", "Jaringan Komputer"];
    public ObservableCollection<string> PilihanFilterAngkatan { get; } = ["Semua Angkatan", "2023", "2024", "2025"];
    public ObservableCollection<string> PilihanFilterStatus { get; } = ["Semua Status", "Aman", "Berisiko"];

    [ObservableProperty] private string _searchPraktikanText = string.Empty;
    [ObservableProperty] private string _selectedFilterKelompok = "Semua Kelompok";
    [ObservableProperty] private string _selectedFilterMatkul = "Semua Mata Kuliah";
    [ObservableProperty] private string _selectedFilterAngkatan = "Semua Angkatan";
    [ObservableProperty] private string _selectedFilterStatus = "Semua Status";
    [ObservableProperty] private string _ringkasanFilterPraktikan = "Menampilkan 124 dari 124 praktikan";
    [ObservableProperty] private bool _isFilterPraktikanKosong;

    public bool IsNavDashboard => ActivePage == "Dashboard";
    public bool IsNavPraktikan => ActivePage is "Praktikan" or "DetailPraktikan";
    public bool IsNavKelompok => ActivePage == "Kelompok";
    public bool IsNavModul => ActivePage == "Modul";
    public bool IsNavPenilaian => ActivePage == "Penilaian";
    public bool IsNavBerisiko => ActivePage == "Berisiko";
    public bool IsNavLogAktivitas => ActivePage == "LogAktivitas";
    public bool IsNavPengguna => ActivePage == "Pengguna";
    public bool IsNavPengaturan => ActivePage == "Pengaturan";

    public bool IsTabSemua => SelectedTabModul == "Semua";
    public bool IsTabBerjalan => SelectedTabModul == "Berjalan";
    public bool IsTabSelesai => SelectedTabModul == "Selesai";
    public bool IsTabBelumDimulai => SelectedTabModul == "Belum Dimulai";

    public bool IsTemaTerang => TemaAktif == "Terang";
    public bool IsTemaGelap => TemaAktif == "Gelap";

    partial void OnActivePageChanged(string value)
    {
        OnPropertyChanged(nameof(IsNavDashboard));
        OnPropertyChanged(nameof(IsNavPraktikan));
        OnPropertyChanged(nameof(IsNavKelompok));
        OnPropertyChanged(nameof(IsNavModul));
        OnPropertyChanged(nameof(IsNavPenilaian));
        OnPropertyChanged(nameof(IsNavBerisiko));
        OnPropertyChanged(nameof(IsNavLogAktivitas));
        OnPropertyChanged(nameof(IsNavPengguna));
        OnPropertyChanged(nameof(IsNavPengaturan));
    }

    partial void OnSelectedTabModulChanged(string value)
    {
        OnPropertyChanged(nameof(IsTabSemua));
        OnPropertyChanged(nameof(IsTabBerjalan));
        OnPropertyChanged(nameof(IsTabSelesai));
        OnPropertyChanged(nameof(IsTabBelumDimulai));
    }

    partial void OnTemaAktifChanged(string value)
    {
        OnPropertyChanged(nameof(IsTemaTerang));
        OnPropertyChanged(nameof(IsTemaGelap));
    }

    // 3. Detail Praktikan
    [ObservableProperty] private DetailPraktikanDto? _selectedDetailPraktikan;

    // 4. Kelompok Praktikum
    public ObservableCollection<RekapKelompokDto> DaftarKelompok { get; } = new();

    // 5. Modul Penugasan
    private List<ModulStatusDto> _allModul = new();
    public ObservableCollection<ModulStatusDto> FilteredModul { get; } = new();
    [ObservableProperty] private string _selectedTabModul = "Semua";

    // 6. Pengumpulan & Penilaian Batch
    public ObservableCollection<ModulStatusDto> PilihanModulPenilaian { get; } = new();
    public ObservableCollection<RekapKelompokDto> PilihanKelompokPenilaian { get; } = new();
    [ObservableProperty] private ModulStatusDto? _selectedModulPenilaian;
    [ObservableProperty] private RekapKelompokDto? _selectedKelompokPenilaian;
    public ObservableCollection<BarisPenilaianViewModel> BarisPenilaian { get; } = new();
    [ObservableProperty] private bool _hasUnsavedGradingChanges;
    [ObservableProperty] private int _jumlahBarisBerubah;

    // 7. Praktikan Berisiko (R1, R2, R3)
    public ObservableCollection<PraktikanBerisikoDto> DaftarBerisiko { get; } = new();
    [ObservableProperty] private int _jumlahR1;
    [ObservableProperty] private int _jumlahR2;
    [ObservableProperty] private int _jumlahR3;

    // 8. Log Aktivitas & Manajemen Pengguna
    public ObservableCollection<LogAktivitasViewDto> DaftarLog { get; } = new();
    [ObservableProperty] private string _filterLogKeyword = string.Empty;
    [ObservableProperty] private string _filterLogAksi = "Semua";
    public ObservableCollection<string> PilihanAksiLog { get; } = ["Semua", "LOGIN", "LOGIN_GAGAL", "NILAI", "KUMPUL", "TAMBAH", "UBAH", "HAPUS"];
    public ObservableCollection<Pengguna> DaftarPengguna { get; } = new();

    // 9. Pengaturan Sistem & Tema
    [ObservableProperty] private string _inputAmbangR2 = "75";
    [ObservableProperty] private string _inputAmbangR3 = "60";
    [ObservableProperty] private string _inputNamaProgram = string.Empty;
    [ObservableProperty] private string _inputPasswordLama = string.Empty;
    [ObservableProperty] private string _inputPasswordBaru = string.Empty;
    [ObservableProperty] private string _indikatorKekuatanPassword = "Kosong";
    [ObservableProperty] private int _skorKekuatanPassword;
    public bool IsPwdBar1 => SkorKekuatanPassword >= 1;
    public bool IsPwdBar2 => SkorKekuatanPassword >= 2;
    public bool IsPwdBar3 => SkorKekuatanPassword >= 3;
    public bool IsPwdBar4 => SkorKekuatanPassword >= 4;
    [ObservableProperty] private string _lokasiDatabaseInfo = string.Empty;

    // ========================================================================
    // 5 DIALOG MODAL STATES
    // ========================================================================

    // Dialog 1: Tambah/Ubah Praktikan
    [ObservableProperty] private bool _isDialogPraktikanOpen;
    [ObservableProperty] private string _judulDialogPraktikan = "Tambah Praktikan";
    [ObservableProperty] private int _formPraktikanId;
    [ObservableProperty] private string _formPraktikanNrp = string.Empty;
    [ObservableProperty] private string _formPraktikanNama = string.Empty;
    [ObservableProperty] private int _formPraktikanAngkatan = 2024;
    [ObservableProperty] private string _formPraktikanMatkul = "Grafika Komputer";
    [ObservableProperty] private RekapKelompokDto? _formPraktikanKelompok;

    // Dialog 2: Tambah/Ubah Kelompok
    [ObservableProperty] private bool _isDialogKelompokOpen;
    [ObservableProperty] private string _judulDialogKelompok = "Tambah Kelompok Praktikum";
    [ObservableProperty] private int _formKelompokId;
    [ObservableProperty] private int _formKelompokNomor = 13;
    [ObservableProperty] private string _formKelompokNama = string.Empty;
    public ObservableCollection<Pengguna> PilihanAsistenTersedia { get; } = new();
    public bool TidakAdaAsistenTersedia => PilihanAsistenTersedia.Count == 0;
    [ObservableProperty] private Pengguna? _formKelompokAsisten;

    // Dialog 3: Tambah/Ubah Modul
    [ObservableProperty] private bool _isDialogModulOpen;
    [ObservableProperty] private string _judulDialogModul = "Tambah Modul Praktikum";
    [ObservableProperty] private int _formModulId;
    [ObservableProperty] private string _formModulKode = "MOD-09";
    [ObservableProperty] private string _formModulJudul = string.Empty;
    [ObservableProperty] private string _formModulKategori = "Modul Mingguan";
    public ObservableCollection<string> PilihanKategoriModul { get; } = ["Modul Mingguan", "Tugas Pendahuluan", "Responsi"];
    [ObservableProperty] private string _formModulMulai = "2026-11-05 08:00";
    [ObservableProperty] private string _formModulTenggat = "2026-11-12 23:59";
    [ObservableProperty] private int _formModulBobot = 15;

    // Dialog 4: Tambah/Ubah Pengguna
    [ObservableProperty] private bool _isDialogPenggunaOpen;
    [ObservableProperty] private string _judulDialogPengguna = "Tambah Akun Pengguna";
    [ObservableProperty] private int _formPenggunaId;
    [ObservableProperty] private string _formPenggunaUsername = string.Empty;
    [ObservableProperty] private string _formPenggunaNama = string.Empty;
    [ObservableProperty] private string _formPenggunaPassword = string.Empty;
    [ObservableProperty] private string _formPenggunaPeran = "Asisten";
    public ObservableCollection<string> PilihanPeranPengguna { get; } = ["Asisten", "Admin"];
    [ObservableProperty] private bool _formPenggunaAktif = true;

    // Dialog 5: Konfirmasi Perubahan Belum Disimpan / Hapus Data
    [ObservableProperty] private bool _isDialogKonfirmasiOpen;
    [ObservableProperty] private string _judulKonfirmasi = "Konfirmasi Tindakan";
    [ObservableProperty] private string _pesanKonfirmasi = string.Empty;
    private Action? _aksiKonfirmasiTertunda;

    public MainWindowViewModel(SimprakDataService dataService, UserSession session, string dbPath)
    {
        _dataService = dataService;
        _session = session;
        _dbPath = dbPath;
        _lokasiDatabaseInfo = dbPath;
        _toastTimer.Tick += (_, _) => TutupToast();

        var pref = PreferensiService.Load();
        if (pref.RememberMe && !string.IsNullOrWhiteSpace(pref.RememberedUsername))
        {
            LoginUsername = pref.RememberedUsername;
            LoginRememberMe = true;
        }
        TemaAktif = pref.LastTheme;
        ThemePaletteService.ApplyTheme(TemaAktif);
    }

    public void TampilkanToast(string pesan, bool isError = false)
    {
        _toastTimer.Stop();
        ToastMessage = pesan;
        IsToastError = isError;
        IsToastVisible = true;
        _toastTimer.Start();
    }

    [RelayCommand]
    public void TutupToast()
    {
        _toastTimer.Stop();
        IsToastVisible = false;
    }

    // ========================================================================
    // LOGIN & LOGOUT COMMANDS
    // ========================================================================

    [RelayCommand]
    public void ProsesLogin()
    {
        LoginErrorMessage = string.Empty;
        var (sukses, pesan) = _dataService.Login(LoginUsername, LoginPassword);
        if (!sukses)
        {
            LoginErrorMessage = pesan;
            return;
        }

        var user = _session.CurrentUser!;
        IsLoggedIn = true;
        IsAdmin = _session.IsAdmin;
        NamaPenggunaAktif = user.NamaLengkap;
        PeranPenggunaAktif = _session.IsAdmin
            ? "Koordinator Lab (Admin)"
            : $"Asisten Lab ({_session.NamaKelompokAsisten ?? "Belum Ditugaskan"})";

        TemaAktif = user.Tema;
        ThemePaletteService.ApplyTheme(TemaAktif);

        PreferensiService.Save(new PreferensiModel
        {
            RememberMe = LoginRememberMe,
            RememberedUsername = LoginRememberMe ? user.Username : string.Empty,
            LastTheme = TemaAktif
        });

        LoginPassword = string.Empty;
        MuatUlangSemuaData();
        NavigasiKe("Dashboard");
        TampilkanToast($"Selamat datang, {user.NamaLengkap}!");
    }

    [RelayCommand]
    public void IsiDemoAdmin()
    {
        LoginUsername = "admin";
        LoginPassword = "admin123";
    }

    [RelayCommand]
    public void IsiDemoAsisten()
    {
        LoginUsername = "asisten01";
        LoginPassword = "asisten123";
    }

    [RelayCommand]
    public void ProsesLogout()
    {
        CekPerubahanSebelumAksi(() =>
        {
            _dataService.Logout();
            IsLoggedIn = false;
            HasUnsavedGradingChanges = false;
        });
    }

    [RelayCommand]
    public void ToggleTema()
    {
        PilihTemaLangsung(TemaAktif == "Terang" ? "Gelap" : "Terang");
    }

    [RelayCommand]
    public void PilihTemaLangsung(string tema)
    {
        if (tema != "Terang" && tema != "Gelap") return;
        TemaAktif = tema;
        ThemePaletteService.ApplyTheme(TemaAktif);
        _dataService.SimpanTemaPengguna(TemaAktif);

        var pref = PreferensiService.Load();
        pref.LastTheme = TemaAktif;
        PreferensiService.Save(pref);
    }

    // ========================================================================
    // NAVIGASI ANTAR 12 LAYAR & PROTEKSI UNSAVED CHANGES
    // ========================================================================

    [RelayCommand]
    public void NavigasiKe(string halamanTujuan)
    {
        if (ActivePage == "Penilaian" && HasUnsavedGradingChanges && halamanTujuan != "Penilaian")
        {
            BukaDialogKonfirmasi(
                "Perubahan Penilaian Belum Disimpan",
                $"Terdapat {JumlahBarisBerubah} baris perubahan penilaian yang belum disimpan. Yakin ingin meninggalkan halaman ini dan membatalkan perubahan?",
                () => EksekusiNavigasi(halamanTujuan));
            return;
        }
        EksekusiNavigasi(halamanTujuan);
    }

    private void EksekusiNavigasi(string halamanTujuan)
    {
        if (!IsAdmin && (halamanTujuan == "LogAktivitas" || halamanTujuan == "Pengguna"))
        {
            TampilkanToast("Halaman tersebut hanya dapat diakses oleh Koordinator Lab (Admin).", true);
            return;
        }

        HasUnsavedGradingChanges = false;
        ActivePage = halamanTujuan;

        (PageTitle, Breadcrumb) = halamanTujuan switch
        {
            "Dashboard" => ("Dashboard Monitoring Praktikum", "Beranda / Dashboard"),
            "Praktikan" => ("Daftar Praktikan & Rekapitulasi", "Akademik / Daftar Praktikan"),
            "DetailPraktikan" => ($"Detail Praktikan — {SelectedDetailPraktikan?.Profil.NamaLengkap}", "Akademik / Daftar Praktikan / Detail"),
            "Kelompok" => ("Kelompok & Shift Praktikum", "Akademik / Kelompok Praktikum"),
            "Modul" => ("Modul & Penugasan Praktikum", "Akademik / Modul Praktikum"),
            "Penilaian" => ("Pengumpulan, Tautan & Penilaian", "Evaluasi / Pengumpulan & Penilaian"),
            "Berisiko" => ("Deteksi Praktikan Rawan Mengulang (BR-05)", "Evaluasi / Praktikan Berisiko"),
            "LogAktivitas" => ("Log Audit Aktivitas Sistem", "Administrasi / Log Aktivitas"),
            "Pengguna" => ("Manajemen Akun & Peran Pengguna", "Administrasi / Pengguna"),
            "Pengaturan" => ("Pengaturan Sistem & Tema", "Sistem / Pengaturan"),
            _ => ("SIMPRAK", "Beranda")
        };

        if (halamanTujuan == "Penilaian")
            MuatTabelPenilaianAktif();
        else if (halamanTujuan == "LogAktivitas")
            MuatDaftarLog();
        else if (halamanTujuan == "Pengguna")
            MuatDaftarPengguna();
    }

    private void CekPerubahanSebelumAksi(Action aksi)
    {
        if (ActivePage == "Penilaian" && HasUnsavedGradingChanges)
        {
            BukaDialogKonfirmasi(
                "Perubahan Belum Disimpan",
                "Anda memiliki perubahan penilaian yang belum disimpan. Lanjutkan dan buang perubahan?",
                aksi);
        }
        else
        {
            aksi();
        }
    }

    public void BukaDialogKonfirmasi(string judul, string pesan, Action aksiSetuju)
    {
        JudulKonfirmasi = judul;
        PesanKonfirmasi = pesan;
        _aksiKonfirmasiTertunda = aksiSetuju;
        IsDialogKonfirmasiOpen = true;
    }

    [RelayCommand]
    public void SetujuKonfirmasi()
    {
        IsDialogKonfirmasiOpen = false;
        var act = _aksiKonfirmasiTertunda;
        _aksiKonfirmasiTertunda = null;
        act?.Invoke();
    }

    [RelayCommand]
    public void BatalKonfirmasi()
    {
        IsDialogKonfirmasiOpen = false;
        _aksiKonfirmasiTertunda = null;
    }

    // ========================================================================
    // PEMUATAN DATA & FILTER REAL-TIME
    // ========================================================================

    [RelayCommand]
    public void MuatUlangSemuaData()
    {
        var (r2, r3, namaProg, _) = _dataService.GetPengaturanSistem();
        InputAmbangR2 = r2.ToString("0.##", CultureInfo.InvariantCulture);
        InputAmbangR3 = r3.ToString("0.##", CultureInfo.InvariantCulture);
        InputNamaProgram = namaProg;
        NamaProgramHeader = namaProg;

        Dashboard = _dataService.GetDashboardData();
        JudulGrafikKetuntasan = IsAdmin
            ? $"Ketuntasan Kelompok Praktikum (Ambang {r2:0.#}%)"
            : $"Ketuntasan per Modul — {_session.NamaKelompokAsisten}";

        // Kelompok
        string prevFilterKel = string.IsNullOrWhiteSpace(SelectedFilterKelompok) ? "Semua Kelompok" : SelectedFilterKelompok;
        SelectedFilterKelompok = string.Empty;
        DaftarKelompok.Clear();
        PilihanFilterKelompok.Clear();
        PilihanFilterKelompok.Add("Semua Kelompok");
        PilihanKelompokPenilaian.Clear();

        foreach (var k in _dataService.GetRekapKelompok())
        {
            DaftarKelompok.Add(k);
            PilihanFilterKelompok.Add(k.NamaKelompok);
            PilihanKelompokPenilaian.Add(k);
        }
        SelectedFilterKelompok = PilihanFilterKelompok.Contains(prevFilterKel) ? prevFilterKel : "Semua Kelompok";

        if (SelectedKelompokPenilaian == null || !PilihanKelompokPenilaian.Any(x => x.IdKelompok == SelectedKelompokPenilaian.IdKelompok))
            SelectedKelompokPenilaian = PilihanKelompokPenilaian.FirstOrDefault();

        // Praktikan
        _allPraktikan = _dataService.GetRekapPraktikanFromSqlView();
        TerapkanFilterPraktikan();

        // Modul
        _allModul = _dataService.GetDaftarModulStatus();
        PilihanModulPenilaian.Clear();
        foreach (var m in _allModul)
            PilihanModulPenilaian.Add(m);

        if (SelectedModulPenilaian == null || !PilihanModulPenilaian.Any(x => x.IdModul == SelectedModulPenilaian.IdModul))
            SelectedModulPenilaian = PilihanModulPenilaian.FirstOrDefault(m => m.StatusModul == "Berjalan") ?? PilihanModulPenilaian.FirstOrDefault();

        TerapkanFilterModul();

        // Praktikan Berisiko
        DaftarBerisiko.Clear();
        var listRisk = _dataService.GetPraktikanBerisiko();
        foreach (var b in listRisk)
            DaftarBerisiko.Add(b);

        JumlahPraktikanBerisikoBadge = listRisk.Count;
        JumlahR1 = listRisk.Count(x => x.R1);
        JumlahR2 = listRisk.Count(x => x.R2);
        JumlahR3 = listRisk.Count(x => x.R3);
    }

    partial void OnGlobalSearchQueryChanged(string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            SearchPraktikanText = value;
            if (ActivePage != "Praktikan")
                NavigasiKe("Praktikan");
        }
    }

    partial void OnSearchPraktikanTextChanged(string value) => TerapkanFilterPraktikan();
    partial void OnSelectedFilterKelompokChanged(string value) => TerapkanFilterPraktikan();
    partial void OnSelectedFilterMatkulChanged(string value) => TerapkanFilterPraktikan();
    partial void OnSelectedFilterAngkatanChanged(string value) => TerapkanFilterPraktikan();
    partial void OnSelectedFilterStatusChanged(string value) => TerapkanFilterPraktikan();

    [RelayCommand]
    public void ResetFilterPraktikan()
    {
        SearchPraktikanText = string.Empty;
        SelectedFilterKelompok = "Semua Kelompok";
        SelectedFilterMatkul = "Semua Mata Kuliah";
        SelectedFilterAngkatan = "Semua Angkatan";
        SelectedFilterStatus = "Semua Status";
        TerapkanFilterPraktikan();
    }

    public void TerapkanFilterPraktikan()
    {
        var q = _allPraktikan.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchPraktikanText))
        {
            string kw = SearchPraktikanText.Trim().ToLowerInvariant();
            q = q.Where(p => p.Nrp.ToLowerInvariant().Contains(kw) || p.NamaLengkap.ToLowerInvariant().Contains(kw));
        }
        if (!string.IsNullOrWhiteSpace(SelectedFilterKelompok) && SelectedFilterKelompok != "Semua Kelompok")
        {
            q = q.Where(p => p.NamaKelompok == SelectedFilterKelompok);
        }
        if (!string.IsNullOrWhiteSpace(SelectedFilterMatkul) && SelectedFilterMatkul != "Semua Mata Kuliah")
        {
            q = q.Where(p => p.MataKuliah == SelectedFilterMatkul);
        }
        if (!string.IsNullOrWhiteSpace(SelectedFilterAngkatan) && SelectedFilterAngkatan != "Semua Angkatan" && int.TryParse(SelectedFilterAngkatan, out int angk))
        {
            q = q.Where(p => p.Angkatan == angk);
        }
        if (!string.IsNullOrWhiteSpace(SelectedFilterStatus) && SelectedFilterStatus != "Semua Status")
        {
            bool cariRisiko = SelectedFilterStatus == "Berisiko";
            q = q.Where(p => p.IsBerisiko == cariRisiko);
        }

        FilteredPraktikan.Clear();
        foreach (var item in q)
            FilteredPraktikan.Add(item);

        RingkasanFilterPraktikan = $"Menampilkan {FilteredPraktikan.Count} dari {_allPraktikan.Count} praktikan";
        IsFilterPraktikanKosong = FilteredPraktikan.Count == 0;
    }

    [RelayCommand]
    public void PilihTabModul(string tab)
    {
        SelectedTabModul = tab;
        TerapkanFilterModul();
    }

    private void TerapkanFilterModul()
    {
        var q = _allModul.AsEnumerable();
        if (SelectedTabModul != "Semua")
            q = q.Where(m => m.StatusModul == SelectedTabModul);

        FilteredModul.Clear();
        foreach (var m in q)
            FilteredModul.Add(m);
    }

    // ========================================================================
    // DETAIL PRAKTIKAN & BUKA TAUTAN (FR-16 & FR-39)
    // ========================================================================

    [RelayCommand]
    public void BukaDetailPraktikan(int idPraktikan)
    {
        var detail = _dataService.GetDetailPraktikan(idPraktikan);
        if (detail == null)
        {
            TampilkanToast("Detail praktikan tidak ditemukan.", true);
            return;
        }
        SelectedDetailPraktikan = detail;
        NavigasiKe("DetailPraktikan");
    }

    [RelayCommand]
    public void BukaTautanEksternal(string? url)
    {
        if (!SimprakRules.ValidasiTautanTugas(url) || string.IsNullOrWhiteSpace(url))
        {
            TampilkanToast("Tautan tugas kosong atau tidak menggunakan protokol HTTP/HTTPS yang valid.", true);
            return;
        }
        bool dibuka = UrlLauncher.BukaUrl(url);
        TampilkanToast(dibuka ? $"Membuka tautan: {url}" : $"Tautan tervalidasi: {url}");
    }

    // ========================================================================
    // EKSPOR CSV (FR-10)
    // ========================================================================

    [RelayCommand]
    public void EksporRekapCsv()
    {
        try
        {
            string csv = CsvExporter.ExportRekapPraktikan(FilteredPraktikan.Count > 0 ? FilteredPraktikan : _allPraktikan);
            string dir = Path.GetDirectoryName(_dbPath)!;
            string fileName = $"Rekap_SIMPRAK_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            string fullPath = Path.Combine(dir, fileName);
            File.WriteAllText(fullPath, csv);
            TampilkanToast($"Rekap CSV berhasil diekspor ke: {fullPath}");
        }
        catch (Exception ex)
        {
            TampilkanToast($"Gagal mengekspor CSV: {ex.Message}", true);
        }
    }

    // ========================================================================
    // CRUD PRAKTIKAN, KELOMPOK, MODUL, & PENGGUNA
    // ========================================================================

    [RelayCommand]
    public void BukaTambahPraktikan()
    {
        JudulDialogPraktikan = "Tambah Praktikan Baru";
        FormPraktikanId = 0;
        FormPraktikanNrp = "5025241200";
        FormPraktikanNama = string.Empty;
        FormPraktikanAngkatan = 2024;
        FormPraktikanMatkul = "Grafika Komputer";
        FormPraktikanKelompok = DaftarKelompok.FirstOrDefault();
        IsDialogPraktikanOpen = true;
    }

    [RelayCommand]
    public void BukaEditPraktikan(RekapPraktikanDto? item)
    {
        if (item == null) return;
        JudulDialogPraktikan = $"Ubah Praktikan — {item.Nrp}";
        FormPraktikanId = item.IdPraktikan;
        FormPraktikanNrp = item.Nrp;
        FormPraktikanNama = item.NamaLengkap;
        FormPraktikanAngkatan = item.Angkatan;
        FormPraktikanMatkul = item.MataKuliah;
        FormPraktikanKelompok = DaftarKelompok.FirstOrDefault(k => k.IdKelompok == item.IdKelompok);
        IsDialogPraktikanOpen = true;
    }

    [RelayCommand]
    public void SimpanPraktikan()
    {
        if (FormPraktikanKelompok == null)
        {
            TampilkanToast("Pilih kelompok praktikum terlebih dahulu.", true);
            return;
        }
        var (sukses, pesan) = _dataService.TambahAtauUbahPraktikan(new Praktikan
        {
            IdPraktikan = FormPraktikanId,
            Nrp = FormPraktikanNrp,
            NamaLengkap = FormPraktikanNama,
            Angkatan = FormPraktikanAngkatan,
            MataKuliah = FormPraktikanMatkul,
            IdKelompok = FormPraktikanKelompok.IdKelompok
        });

        if (!sukses)
        {
            TampilkanToast(pesan, true);
            return;
        }
        IsDialogPraktikanOpen = false;
        MuatUlangSemuaData();
        TampilkanToast(pesan);
    }

    [RelayCommand]
    public void HapusPraktikanItem(RekapPraktikanDto? item)
    {
        if (item == null) return;
        BukaDialogKonfirmasi(
            "Hapus Data Praktikan",
            $"Yakin ingin menghapus praktikan {item.Nrp} - {item.NamaLengkap}? (Praktikan yang sudah memiliki riwayat pengumpulan akan ditolak penghapusannya).",
            () =>
            {
                var (sukses, pesan) = _dataService.HapusPraktikan(item.IdPraktikan);
                TampilkanToast(pesan, !sukses);
                if (sukses) MuatUlangSemuaData();
            });
    }

    [RelayCommand]
    public void BukaTambahKelompok()
    {
        JudulDialogKelompok = "Tambah Kelompok Praktikum";
        FormKelompokId = 0;
        FormKelompokNomor = DaftarKelompok.Count + 1;
        FormKelompokNama = $"Shift E1 - Kelompok {DaftarKelompok.Count + 1}";
        MuatPilihanAsisten(null);
        FormKelompokAsisten = PilihanAsistenTersedia.FirstOrDefault();
        IsDialogKelompokOpen = true;
    }

    [RelayCommand]
    public void BukaEditKelompok(RekapKelompokDto? item)
    {
        if (item == null) return;
        JudulDialogKelompok = $"Ubah Kelompok #{item.NomorKelompok}";
        FormKelompokId = item.IdKelompok;
        FormKelompokNomor = item.NomorKelompok;
        FormKelompokNama = item.NamaKelompok;
        MuatPilihanAsisten(item.IdKelompok);
        FormKelompokAsisten = PilihanAsistenTersedia.FirstOrDefault(a => a.IdPengguna == item.IdAsisten);
        IsDialogKelompokOpen = true;
    }

    private void MuatPilihanAsisten(int? idKelompokEdit)
    {
        PilihanAsistenTersedia.Clear();
        foreach (var a in _dataService.GetAsistenTersediaUntukKelompok(idKelompokEdit))
            PilihanAsistenTersedia.Add(a);
        OnPropertyChanged(nameof(TidakAdaAsistenTersedia));
    }

    [RelayCommand]
    public void KosongkanAsistenKelompok() => FormKelompokAsisten = null;

    [RelayCommand]
    public void SimpanKelompok()
    {
        var (sukses, pesan) = _dataService.TambahAtauUbahKelompok(new Kelompok
        {
            IdKelompok = FormKelompokId,
            NomorKelompok = FormKelompokNomor,
            NamaKelompok = FormKelompokNama,
            IdAsisten = FormKelompokAsisten?.IdPengguna
        });
        if (!sukses)
        {
            TampilkanToast(pesan, true);
            return;
        }
        IsDialogKelompokOpen = false;
        MuatUlangSemuaData();
        TampilkanToast(pesan);
    }

    [RelayCommand]
    public void HapusKelompokItem(RekapKelompokDto? item)
    {
        if (item == null) return;
        var (sukses, pesan) = _dataService.HapusKelompok(item.IdKelompok);
        TampilkanToast(pesan, !sukses);
        if (sukses) MuatUlangSemuaData();
    }

    [RelayCommand]
    public void BukaTambahModul()
    {
        JudulDialogModul = "Tambah Modul Penugasan";
        FormModulId = 0;
        FormModulKode = $"MOD-0{_allModul.Count + 1}";
        FormModulJudul = string.Empty;
        FormModulKategori = "Modul Mingguan";
        FormModulMulai = "2026-11-01 08:00";
        FormModulTenggat = "2026-11-08 23:59";
        FormModulBobot = 15;
        IsDialogModulOpen = true;
    }

    [RelayCommand]
    public void BukaEditModul(ModulStatusDto? item)
    {
        if (item == null) return;
        JudulDialogModul = $"Ubah Modul {item.KodeModul}";
        FormModulId = item.IdModul;
        FormModulKode = item.KodeModul;
        FormModulJudul = item.JudulModul;
        FormModulKategori = item.Kategori;
        FormModulMulai = item.TanggalMulai;
        FormModulTenggat = item.Tenggat;
        FormModulBobot = item.Bobot;
        IsDialogModulOpen = true;
    }

    [RelayCommand]
    public void SimpanModul()
    {
        var (sukses, pesan) = _dataService.TambahAtauUbahModul(new Modul
        {
            IdModul = FormModulId,
            KodeModul = FormModulKode,
            JudulModul = FormModulJudul,
            Kategori = FormModulKategori,
            TanggalMulai = FormModulMulai,
            Tenggat = FormModulTenggat,
            Bobot = FormModulBobot
        });
        if (!sukses)
        {
            TampilkanToast(pesan, true);
            return;
        }
        IsDialogModulOpen = false;
        MuatUlangSemuaData();
        TampilkanToast(pesan);
    }

    [RelayCommand]
    public void PintasanNilaiModul(ModulStatusDto? item)
    {
        if (item == null) return;
        SelectedModulPenilaian = PilihanModulPenilaian.FirstOrDefault(m => m.IdModul == item.IdModul);
        NavigasiKe("Penilaian");
    }

    // ========================================================================
    // PENGUMPULAN & PENILAIAN INTERAKTIF
    // ========================================================================

    partial void OnSelectedModulPenilaianChanged(ModulStatusDto? value)
    {
        if (ActivePage == "Penilaian" && value != null)
            MuatTabelPenilaianAktif();
    }

    partial void OnSelectedKelompokPenilaianChanged(RekapKelompokDto? value)
    {
        if (ActivePage == "Penilaian" && value != null)
            MuatTabelPenilaianAktif();
    }

    public void MuatTabelPenilaianAktif()
    {
        if (SelectedModulPenilaian == null || SelectedKelompokPenilaian == null)
            return;

        BarisPenilaian.Clear();
        var rows = _dataService.GetTabelPenilaian(SelectedModulPenilaian.IdModul, SelectedKelompokPenilaian.IdKelompok);
        foreach (var r in rows)
        {
            BarisPenilaian.Add(new BarisPenilaianViewModel(r, HitungStatusDirtyPenilaian));
        }
        HitungStatusDirtyPenilaian();
    }

    private void HitungStatusDirtyPenilaian()
    {
        JumlahBarisBerubah = BarisPenilaian.Count(x => x.IsDirty);
        HasUnsavedGradingChanges = JumlahBarisBerubah > 0;
    }

    [RelayCommand]
    public void TandaiSemuaTerkumpul()
    {
        foreach (var row in BarisPenilaian)
            row.TandaiTerkumpulSekarang();
        HitungStatusDirtyPenilaian();
        TampilkanToast("Seluruh praktikan pada kelompok ini ditandai terkumpul. Klik 'Simpan Semua' untuk menyimpan.");
    }

    [RelayCommand]
    public void BatalkanPerubahanPenilaian()
    {
        foreach (var row in BarisPenilaian)
            row.ResetKeAsal();
        HitungStatusDirtyPenilaian();
        TampilkanToast("Perubahan penilaian yang belum disimpan telah dibatalkan.");
    }

    [RelayCommand]
    public void SimpanSemuaPenilaian()
    {
        var dtos = new List<InputPenilaianBarisDto>();
        foreach (var row in BarisPenilaian.Where(r => r.IsDirty))
        {
            var (valid, err, dto) = row.ToDto();
            if (!valid || dto == null)
            {
                TampilkanToast(err, true);
                return;
            }
            dtos.Add(dto);
        }

        if (dtos.Count == 0)
        {
            TampilkanToast("Tidak ada perubahan baru untuk disimpan.");
            return;
        }

        var (sukses, pesan, _) = _dataService.SimpanPenilaianBatch(dtos);
        if (!sukses)
        {
            TampilkanToast(pesan, true);
            return;
        }

        MuatUlangSemuaData();
        MuatTabelPenilaianAktif();
        TampilkanToast(pesan);
    }

    // ========================================================================
    // LOG AKTIVITAS, PENGGUNA, & PENGATURAN
    // ========================================================================

    partial void OnFilterLogKeywordChanged(string value) => MuatDaftarLog();
    partial void OnFilterLogAksiChanged(string value) => MuatDaftarLog();

    [RelayCommand]
    public void MuatDaftarLog()
    {
        DaftarLog.Clear();
        foreach (var l in _dataService.GetLogAktivitas(FilterLogKeyword, null, FilterLogAksi))
            DaftarLog.Add(l);
    }

    public void MuatDaftarPengguna()
    {
        DaftarPengguna.Clear();
        foreach (var u in _dataService.GetDaftarPengguna())
            DaftarPengguna.Add(u);
    }

    [RelayCommand]
    public void BukaTambahPengguna()
    {
        JudulDialogPengguna = "Tambah Akun Pengguna Baru";
        FormPenggunaId = 0;
        FormPenggunaUsername = string.Empty;
        FormPenggunaNama = string.Empty;
        FormPenggunaPassword = string.Empty;
        FormPenggunaPeran = "Asisten";
        FormPenggunaAktif = true;
        IsDialogPenggunaOpen = true;
    }

    [RelayCommand]
    public void BukaEditPengguna(Pengguna? item)
    {
        if (item == null) return;
        JudulDialogPengguna = $"Ubah Pengguna — {item.Username}";
        FormPenggunaId = item.IdPengguna;
        FormPenggunaUsername = item.Username;
        FormPenggunaNama = item.NamaLengkap;
        FormPenggunaPassword = string.Empty;
        FormPenggunaPeran = item.Peran;
        FormPenggunaAktif = item.StatusAktif == 1;
        IsDialogPenggunaOpen = true;
    }

    [RelayCommand]
    public void SimpanPengguna()
    {
        var (sukses, pesan) = _dataService.TambahAtauUbahPengguna(new Pengguna
        {
            IdPengguna = FormPenggunaId,
            Username = FormPenggunaUsername,
            NamaLengkap = FormPenggunaNama,
            Peran = FormPenggunaPeran,
            Tema = TemaAktif,
            StatusAktif = FormPenggunaAktif ? 1 : 0
        }, FormPenggunaPassword);

        if (!sukses)
        {
            TampilkanToast(pesan, true);
            return;
        }
        IsDialogPenggunaOpen = false;
        MuatDaftarPengguna();
        MuatUlangSemuaData();
        if (IsDialogKelompokOpen)
        {
            int? asistenSebelumnya = FormKelompokAsisten?.IdPengguna;
            MuatPilihanAsisten(FormKelompokId == 0 ? null : FormKelompokId);
            FormKelompokAsisten = PilihanAsistenTersedia.FirstOrDefault(a =>
                string.Equals(a.Username, FormPenggunaUsername.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? PilihanAsistenTersedia.FirstOrDefault(a => a.IdPengguna == asistenSebelumnya);
        }
        TampilkanToast(pesan);
    }

    partial void OnInputPasswordBaruChanged(string value)
    {
        var (level, skor) = SimprakRules.EvaluasiKekuatanPassword(value);
        IndikatorKekuatanPassword = level;
        SkorKekuatanPassword = skor;
        OnPropertyChanged(nameof(IsPwdBar1));
        OnPropertyChanged(nameof(IsPwdBar2));
        OnPropertyChanged(nameof(IsPwdBar3));
        OnPropertyChanged(nameof(IsPwdBar4));
    }

    [RelayCommand]
    public void SimpanPengaturanAmbang()
    {
        if (!double.TryParse(InputAmbangR2.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double r2) ||
            !double.TryParse(InputAmbangR3.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out double r3))
        {
            TampilkanToast("Nilai ambang R2 dan R3 harus berupa angka.", true);
            return;
        }

        var (sukses, pesan) = _dataService.SimpanAmbangRisiko(r2, r3, InputNamaProgram);
        TampilkanToast(pesan, !sukses);
        if (sukses) MuatUlangSemuaData();
    }

    [RelayCommand]
    public void ProsesGantiPassword()
    {
        var (sukses, pesan) = _dataService.GantiPassword(InputPasswordLama, InputPasswordBaru);
        TampilkanToast(pesan, !sukses);
        if (sukses)
        {
            InputPasswordLama = string.Empty;
            InputPasswordBaru = string.Empty;
        }
    }

    [RelayCommand]
    public void ResetDataContoh()
    {
        BukaDialogKonfirmasi(
            "Pulihkan Data Contoh (Seed Data)",
            "Seluruh perubahan pada database lokal akan dikembalikan ke data awal (124 praktikan, 12 kelompok, 8 modul). Lanjutkan?",
            () =>
            {
                DatabaseInitializer.ResetDatabase(_dbPath);
                MuatUlangSemuaData();
                TampilkanToast("Database berhasil dipulihkan ke data contoh awal.");
            });
    }

    [RelayCommand]
    public void TutupSemuaDialog()
    {
        if (IsDialogKelompokOpen && IsDialogPenggunaOpen)
        {
            IsDialogPenggunaOpen = false;
            return;
        }
        IsDialogPraktikanOpen = false;
        IsDialogKelompokOpen = false;
        IsDialogModulOpen = false;
        IsDialogPenggunaOpen = false;
        IsDialogKonfirmasiOpen = false;
    }
}
