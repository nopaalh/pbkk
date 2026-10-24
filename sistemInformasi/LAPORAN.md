# Project SIMPRAK (Sistem Informasi Monitoring dan Penilaian Praktikum)

**Nama:** Muhammad Naufal Hadaya Setiawan  
**NRP:** 5025241181

Dokumentasi pengembangan SIMPRAK, aplikasi desktop untuk mengelola praktikan, kelompok, modul, pengumpulan tugas, dan penilaian praktikum.

<!-- Penanda GAMBAR dapat diganti dengan gambar Markdown, misalnya ![Halaman login](gambar/login.png). Teks petunjuk pada blok gambar dapat dihapus setelah gambar dimasukkan. -->

## 1. Ringkasan proyek

SIMPRAK (Sistem Informasi Monitoring dan Penilaian Praktikum) membantu koordinator laboratorium dan asisten laboratorium memantau kegiatan praktikum perkuliahan. Aplikasi menyatukan data praktikan, kelompok, modul, pengumpulan, dan penilaian dalam satu tempat. Status pengumpulan, ketuntasan, nilai akhir berbobot, serta indikator praktikan berisiko dihitung dari sumber data yang sama sehingga rekap tidak perlu digabungkan secara manual dari banyak spreadsheet.

SIMPRAK dikembangkan sebagai aplikasi desktop dengan C#, .NET 10, dan Avalonia UI. Antarmuka menggunakan XAML dan pola Model–View–ViewModel (MVVM). Akses data menggunakan Entity Framework Core serta SQLite. Data tersimpan secara lokal pada satu komputer dan belum disinkronkan antarperangkat.

Target platform proyek adalah Windows dan Linux. Pemeriksaan yang tercatat selama pengembangan dilakukan pada lingkungan Linux, termasuk pengujian otomatis dan pemeriksaan tampilan melalui Avalonia.Headless. Pengujian paket distribusi Windows secara terpisah masih perlu dilakukan.

**Tagline:** Setiap progres praktikan, terpantau akurat.

| Tautan project | Lokasi |
| --- | --- |
| Repository GitHub | [Isi tautan repository GitHub] |
| Product Requirements Document | [PRD SIMPRAK](PRD.md) |
| Desain antarmuka di Figma | [Isi tautan desain Figma jika tersedia] |
| Unduhan aplikasi | [Isi tautan paket aplikasi yang sudah diterbitkan] |
| Video presentasi dan demonstrasi | [Isi tautan video] |

## 2. Latar belakang dan tujuan

Dalam penyelenggaraan praktikum, mahasiswa dibagi ke dalam kelompok atau shift dan menerima beberapa modul selama satu periode perkuliahan. Asisten mencatat waktu pengumpulan serta memberikan nilai. Koordinator laboratorium membutuhkan rekap seluruh kelompok untuk memantau progres dan menentukan praktikan yang memerlukan perhatian.

Pencatatan melalui spreadsheet terpisah dapat menimbulkan perbedaan format, rumus, dan penentuan status terlambat. Perubahan nilai juga sulit diperiksa apabila catatan hanya menyimpan nilai terakhir. Selain itu, pembagian berkas belum tentu membatasi siapa yang boleh mengubah data kelompok tertentu.

SIMPRAK dikembangkan untuk:

- Mengelola akun pengguna, kelompok praktikum, praktikan, dan modul penugasan.
- Mencatat waktu pengumpulan dan tautan hasil tugas, kemudian menentukan status pengumpulan berdasarkan tenggat.
- Menyimpan nilai dan catatan evaluasi, termasuk perubahan beberapa baris dalam satu transaksi.
- Menghitung ketuntasan serta nilai akhir berbobot.
- Menandai praktikan yang memenuhi kriteria risiko R1, R2, atau R3.
- Menampilkan dashboard, grafik, tabel rekap, dan aktivitas terbaru.
- Membatasi pengelolaan data asisten sesuai kelompok bimbingannya.
- Menyimpan log aktivitas, termasuk nilai sebelum dan sesudah perubahan.
- Menyediakan tema terang dan gelap, ekspor CSV, serta notifikasi tindakan yang hilang otomatis.

Pembatasan hak akses merupakan kebutuhan produk. Beberapa jalur layanan yang masih memerlukan penguatan dicatat pada bagian 11 agar cakupan implementasi tidak disamakan dengan seluruh kebutuhan yang ditetapkan.

## 3. Kebutuhan dan aturan bisnis

Kebutuhan aplikasi didokumentasikan dalam [PRD SIMPRAK](PRD.md). Dokumen tersebut memuat ruang lingkup, peran pengguna, 41 kebutuhan fungsional, kebutuhan nonfungsional, aturan bisnis, data, rancangan layar, arsitektur, dan 19 skenario uji penerimaan.

FR-39 mengatur penyimpanan dan validasi tautan tugas. FR-40 mengatur notifikasi tindakan selama 2 detik. FR-41 mengatur keterbacaan antarmuka, termasuk label filter, ukuran teks, scrollbar tabel, dan tooltip. PRD juga mencatat perbedaan antara kebutuhan yang ditetapkan dan implementasi saat ini.

Aturan bisnis utama aplikasi meliputi:

| Aturan | Ketentuan |
| --- | --- |
| BR-01, status pengumpulan | Tepat Waktu jika waktu pengumpulan tidak melewati tenggat, Terlambat jika melewati tenggat, dan Belum Mengumpulkan jika waktu pengumpulan kosong. |
| BR-02, integritas data | NRP harus unik dan terdiri atas 10 digit. Praktikan yang mempunyai riwayat pengumpulan tidak dapat dihapus. Kelompok yang masih beranggota tidak dapat dihapus. Admin tidak dapat menonaktifkan atau mengubah perannya sendiri. |
| BR-03, ketuntasan | Jumlah modul jatuh tempo yang dikumpulkan dibagi jumlah seluruh modul jatuh tempo, kemudian dikalikan 100%. Hasil dibulatkan menjadi dua angka desimal. |
| BR-04, nilai akhir | Nilai akhir merupakan rata-rata berbobot. Modul jatuh tempo yang belum dikumpulkan dihitung bernilai nol. Pengumpulan yang sudah masuk tetapi belum dinilai tidak masuk perhitungan. |
| BR-05, praktikan berisiko | Praktikan ditandai berisiko jika memenuhi sedikitnya satu kriteria R1, R2, atau R3. Semua alasan yang terpenuhi ditampilkan. |

Kriteria risiko yang digunakan adalah:

- R1, sedikitnya dua modul jatuh tempo belum dikumpulkan.
- R2, ketuntasan lebih rendah dari ambang yang ditentukan Admin. Nilai awalnya 75%.
- R3, nilai akhir lebih rendah dari ambang yang ditentukan Admin dan sedikitnya dua modul sudah dinilai. Nilai awalnya 60.

Ambang R2 dan R3 dapat diubah pada rentang 10 sampai 100. Jumlah minimum dua modul pada R1 merupakan aturan tetap versi ini.

Ketuntasan bernilai 100% jika belum ada modul jatuh tempo. Nilai akhir bernilai nol jika belum ada bobot yang masuk perhitungan. Modul yang sudah dinilai tetap masuk rata-rata meskipun tenggatnya belum lewat. Keterlambatan tidak otomatis mengurangi nilai numerik.

Sebagai contoh, nilai 90 pada modul berbobot 15 dan satu modul belum dikumpulkan yang sudah jatuh tempo dengan bobot 15 menghasilkan nilai akhir 45. Jika modul kedua sudah dikumpulkan tetapi belum dinilai, nilai akhirnya tetap 90.

Modul berlaku untuk seluruh kelompok dan praktikan pada database. Pemilihan kelompok di halaman penilaian berfungsi menyaring praktikan yang akan dinilai, bukan menentukan kelompok penerima modul.

Kegiatan pengembangan mencakup analisis kebutuhan, pendokumentasian PRD, perancangan data dan antarmuka, implementasi aturan bisnis serta layanan, pengujian, dan revisi berdasarkan temuan penggunaan. Perbaikan terakhir mencakup keterbacaan, timer notifikasi, serta alur penambahan asisten dari formulir kelompok.

## 4. Desain antarmuka

Antarmuka SIMPRAK menggunakan latar netral, sidebar hijau gelap, dan aksen hijau untuk tombol utama, navigasi, serta sebagian grafik. Warna status selalu disertai tulisan agar informasi tidak bergantung pada warna saja.

Tema terang dan gelap memiliki pengaturan warna masing-masing. Ukuran teks, batas komponen, warna tombol, dan jarak antarelemen disesuaikan agar isi formulir serta tabel lebih mudah dibaca.

<!-- GAMBAR 01 -->
> **Gambar 1. Gambaran desain antarmuka SIMPRAK**  
> [Sisipkan gambar rancangan antarmuka atau kumpulan tampilan aplikasi di sini.]

### 4.1 Palet warna

| Elemen | Tema terang | Tema gelap | Penggunaan |
| --- | --- | --- | --- |
| Aksen utama | `#456D1B` | `#A3D769` | Tombol utama, menu aktif, dan tautan |
| Teks tombol utama | `#FFFFFF` | `#14230B` | Tulisan pada tombol beraksen |
| Sidebar | `#111C14` | `#09100B` | Navigasi samping |
| Latar halaman | `#F4F7F2` | `#0D1510` | Latar konten |
| Permukaan | `#FFFFFF` | `#152219` | Kartu, panel, dan dialog |
| Teks utama | `#111C14` | `#F1F5F0` | Judul, isi, dan nilai |
| Teks pendukung | `#435148` | `#C0CEC2` | Keterangan, label, dan informasi tambahan |
| Berhasil | `#15803D` | `#4ADE80` | Tepat Waktu, Aman, dan Aktif |
| Peringatan | `#B45309` | `#FBBF24` | Terlambat dan perubahan belum disimpan |
| Bahaya | `#B91C1C` | `#F87171` | Belum Mengumpulkan, Berisiko, dan galat |
| Informasi | `#0369A1` | `#38BDF8` | Informasi umum dan status lain |

Grafik kustom menggunakan warna dan gradasi yang ditentukan pada komponen grafik. Warna tersebut tidak seluruhnya mengambil nilai aksen tombol.

### 4.2 Tipografi dan tata letak

Aplikasi memakai font Inter melalui paket Avalonia.Fonts.Inter. Teks isi umumnya berukuran 14 px, sedangkan label penting sekurangnya 13 px. Ukuran judul dan angka ringkasan lebih besar agar hierarki informasi terlihat.

Sidebar menampilkan identitas aplikasi, navigasi sesuai peran, dan profil pengguna. Header memuat judul halaman, breadcrumb, pencarian praktikan dengan pintasan Ctrl+K, pengaturan tema, serta ekspor CSV. Konten utama berisi kartu statistik, grafik, tabel, dan formulir.

Ukuran awal jendela adalah 1420 × 860 dengan ukuran minimum 1280 × 740. Tabel yang lebar dapat digeser secara horizontal. Tabel panjang memiliki area gulir vertikal, sementara tooltip membantu membaca teks yang terpotong.

Dialog digunakan untuk menambah atau mengubah praktikan, kelompok, modul, dan pengguna. Dialog konfirmasi dipakai untuk tindakan yang memerlukan persetujuan pengguna. Notifikasi hasil tindakan tampil selama 2 detik dan dapat ditutup lebih cepat secara manual.

## 5. Teknologi dan arsitektur

SIMPRAK menggunakan teknologi desktop dan database lokal agar fitur inti dapat dijalankan tanpa memasang server.

| Teknologi | Versi atau konfigurasi | Penggunaan |
| --- | --- | --- |
| C# dan .NET | Target `net10.0` | Bahasa serta platform aplikasi |
| Avalonia UI | 11.2.3 | Antarmuka desktop, XAML, binding, style, dan rendering |
| CommunityToolkit.Mvvm | 8.3.2 | Properti teramati dan perintah melalui `[ObservableProperty]` serta `[RelayCommand]` |
| Entity Framework Core SQLite | 9.0.0 | Pemetaan entitas dan pengelolaan data |
| SQLite | Database lokal | Tabel, constraint, indeks, trigger, dan view laporan |
| Microsoft.Extensions.DependencyInjection | 9.0.0 | Penyediaan sesi, layanan data, dan ViewModel |
| BCrypt.Net-Next | 4.0.3 | Hashing dan verifikasi password |
| Avalonia.Fonts.Inter | 11.2.3 | Penyertaan font Inter |
| xUnit | 2.9.3 | Pengujian otomatis |
| Avalonia.Headless | 11.2.3 | Pengujian perilaku UI tanpa membuka jendela desktop |

Struktur kode memisahkan tampilan, aturan inti, dan akses data. View mendefinisikan tata letak serta binding. ViewModel mengelola keadaan layar dan meneruskan tindakan. Layanan data menjalankan validasi, pemeriksaan akses, penyimpanan, dan pencatatan aktivitas.

```text
Pengguna
  ↓
View Avalonia (XAML)
  ↓ data binding dan command
MainWindowViewModel
  ↓
SimprakDataService ← UserSession
  ↓
EF Core dan query SQLite
  ↓
SQLite lokal (simprak.db)

Simprak.Core menyediakan model, DTO, aturan bisnis,
sesi pengguna, dan pembentukan CSV.
```

Saat aplikasi mulai, dependency injection menyiapkan `UserSession`, `SimprakDataService`, dan `MainWindowViewModel`. ViewModel dipasang sebagai `DataContext` pada jendela utama.

Pengelolaan data menggunakan EF Core. Sebagian besar rekap dibaca melalui query ke view SQLite. Perhitungan C# digunakan sebagai pembanding dalam pengujian konsistensi. Komponen grafik batang, grafik garis, dan progress ring menggambar visual melalui metode `Render` Avalonia.

### 5.1 Struktur proyek

Solution `Simprak.slnx` memuat proyek aplikasi, inti, akses data, dan pengujian.

```text
SIMPRAK/
├── Simprak.slnx
├── README.md
├── PRD.md
├── LAPORAN.md
├── database/
│   ├── simprak.sql
│   └── PDM_SIMPRAK.png
├── src/
│   ├── Simprak.Core/
│   │   ├── Models/
│   │   │   ├── Entities.cs
│   │   │   └── ViewModelsDto.cs
│   │   ├── Rules/
│   │   │   └── SimprakRules.cs
│   │   └── Services/
│   │       └── SessionAndCsv.cs
│   ├── Simprak.Data/
│   │   ├── SimprakDbContext.cs
│   │   ├── DatabaseInitializer.cs
│   │   └── Services/
│   │       └── SimprakDataService.cs
│   └── Simprak.App/
│       ├── App.axaml
│       ├── App.axaml.cs
│       ├── Program.cs
│       ├── Views/
│       ├── ViewModels/
│       ├── Controls/
│       ├── Converters/
│       └── Services/
└── tests/
    └── Simprak.Tests/
        ├── SimprakComprehensiveTests.cs
        ├── ToastTests.cs
        └── KelompokDialogTests.cs
```

`Simprak.Core` tidak bergantung pada proyek aplikasi atau data. `Simprak.Data` menggunakan `Simprak.Core`. `Simprak.App` menggunakan keduanya. Proyek pengujian memeriksa aturan, layanan data, dan perilaku ViewModel aplikasi.

## 6. Perancangan database

Database SQLite terdiri atas tujuh tabel, sembilan indeks eksplisit, tujuh trigger, dan enam view. Status pengumpulan, status modul, ketuntasan, nilai akhir, dan status risiko dihitung dari data sumber, bukan disimpan sebagai kolom hasil yang harus diperbarui secara terpisah.

<!-- GAMBAR 02 -->
> **Gambar 2. Physical Data Model SIMPRAK**  
> [Sisipkan gambar PDM di sini. Berkas referensi proyek tersedia pada `database/PDM_SIMPRAK.png`.]

Tabel utama database meliputi:

| Tabel | Isi dan ketentuan utama |
| --- | --- |
| `pengguna` | Username unik, nama lengkap, hash password, peran Admin atau Asisten, tema, status aktif, dan waktu login terakhir. |
| `kelompok` | Nomor dan nama kelompok unik serta asisten pembimbing opsional. Satu asisten membimbing paling banyak satu kelompok. |
| `praktikan` | NRP unik 10 digit, nama, angkatan, mata kuliah, dan kelompok. Penghapusan ditolak jika sudah memiliki pengumpulan. |
| `modul` | Kode unik, judul, kategori, tanggal mulai, tenggat, dan bobot 1 sampai 100. Tenggat tidak boleh sebelum tanggal mulai. |
| `pengumpulan` | Pasangan praktikan dan modul yang unik, waktu kumpul, tautan opsional, nilai, catatan, penilai, serta waktu penilaian. |
| `log_aktivitas` | Pelaku, aksi, entitas, ID entitas, keterangan, dan waktu. ID entitas tidak menjadi foreign key ke objek aktivitas sehingga identitas objek tetap dapat dicatat setelah objek dihapus. |
| `pengaturan` | Konfigurasi berbentuk pasangan kunci dan nilai, termasuk nama program, ambang risiko, dan waktu acuan. |

Kategori modul yang tersedia adalah Modul Mingguan, Tugas Pendahuluan, dan Responsi. Kolom nilai pada pengumpulan dapat kosong atau berisi angka dari 0 sampai 100. Tautan bersifat opsional dan divalidasi sebagai URL HTTP atau HTTPS pada layanan aplikasi.

Relasi utama database adalah kelompok memiliki banyak praktikan, praktikan memiliki banyak pengumpulan, dan modul memiliki banyak pengumpulan. Pengguna dapat menjadi pembimbing, penilai, dan pelaku aktivitas. Trigger memeriksa bahwa pembimbing merupakan Asisten aktif dan penilai memiliki hak untuk menilai praktikan terkait.

Enam view yang digunakan untuk perhitungan adalah `v_status_pengumpulan`, `v_modul_status`, `v_rekap_praktikan`, `v_praktikan_berisiko`, `v_rekap_kelompok`, dan `v_dashboard`. Dashboard aplikasi juga menyusun hasil dari beberapa layanan rekap untuk membentuk grafik serta aktivitas terbaru.

View status pengumpulan memasangkan seluruh praktikan dengan seluruh modul, kemudian menggabungkan catatan pengumpulan yang ada. Dengan cara ini, praktikan yang belum mengumpulkan tetap muncul dalam rekap. Belum ada relasi yang membatasi modul pada kelompok atau mata kuliah tertentu.

Saat pertama dijalankan, aplikasi membuat database dengan data contoh berisi 124 praktikan, 12 kelompok, dan 8 modul. Database yang sudah ada tidak diisi ulang pada setiap pembukaan aplikasi. Pemeriksaan kompatibilitas skema dapat menambahkan kolom `tautan_tugas` pada database lama tanpa menghapus data sebelumnya.

## 7. Fitur aplikasi

### 7.1 Login dan hak akses

Halaman login menerima username dan password serta menyediakan pilihan untuk mengingat username pada perangkat. Akun demo dapat diisikan melalui tombol yang tersedia. Jika autentikasi gagal, aplikasi menampilkan pesan umum "Username atau password salah" dan mencatat percobaan tersebut.

Setelah login, aplikasi menyimpan identitas pengguna dan kelompok bimbingan pada sesi di memori. Menu Log Aktivitas serta Pengguna dibatasi untuk Admin. Sejumlah operasi data juga memeriksa peran dan kelompok pengguna sebelum melanjutkan tindakan.

<!-- GAMBAR 03 -->
> **Gambar 3. Halaman login SIMPRAK**  
> [Sisipkan tangkapan layar halaman login di sini.]

### 7.2 Dashboard

Dashboard menampilkan jumlah praktikan, jumlah kelompok, modul berjalan, dan rata-rata ketuntasan. Grafik ketuntasan kelompok membantu koordinator membandingkan progres. Distribusi nilai akhir ditampilkan dalam lima rentang, yaitu di bawah 50, 50 sampai kurang dari 60, 60 sampai kurang dari 70, 70 sampai kurang dari 85, serta 85 sampai 100.

Bagian lain menampilkan praktikan berisiko dan aktivitas terbaru. Untuk Asisten yang mempunyai kelompok, grafik ketuntasan menampilkan pengumpulan per modul pada kelompoknya. Tombol pada dashboard memberikan akses menuju penilaian dan penambahan praktikan.

Sebagian indikator dashboard masih menggunakan nilai contoh tetap. Garis ambang grafik juga masih memakai nilai 75%. Perbedaan tersebut menjadi catatan perbaikan ketika data atau ambang risiko berubah.

<!-- GAMBAR 04 -->
> **Gambar 4. Dashboard SIMPRAK**  
> [Sisipkan tangkapan layar dashboard di sini.]

### 7.3 Praktikan dan detail praktikan

Daftar praktikan memuat NRP, nama, kelompok, mata kuliah, angkatan, ketuntasan, nilai akhir, dan status risiko. Pengguna dapat mencari berdasarkan nama atau NRP serta memfilter berdasarkan kelompok, mata kuliah, angkatan, dan status.

Formulir memvalidasi NRP, nama, angkatan, mata kuliah, dan kelompok. NRP ganda ditolak. Praktikan yang sudah mempunyai riwayat pengumpulan tidak dapat dihapus. Versi ini belum menyediakan penonaktifan praktikan sebagai pengganti penghapusan.

Detail praktikan menampilkan profil, ringkasan nilai, ketuntasan, keterlambatan, alasan risiko, dan riwayat pengumpulan setiap modul. Grafik membandingkan nilai praktikan dengan rata-rata kelompok. Tautan tugas dapat dibuka dari halaman detail.

<!-- GAMBAR 05 -->
> **Gambar 5. Daftar praktikan**  
> [Sisipkan tangkapan layar daftar praktikan beserta filter di sini.]

<!-- GAMBAR 06 -->
> **Gambar 6. Detail praktikan**  
> [Sisipkan tangkapan layar profil, grafik, dan riwayat modul praktikan di sini.]

### 7.4 Kelompok dan modul

Halaman kelompok menampilkan asisten pembimbing, jumlah anggota, nilai rata-rata, ketuntasan, dan praktikan berisiko. Admin dapat menambah serta mengubah kelompok. Penghapusan kelompok yang masih mempunyai anggota ditolak.

Pilihan pembimbing hanya memuat Asisten aktif yang belum membimbing kelompok lain. Saat kelompok diedit, asisten pembimbingnya tetap disertakan dalam pilihan. Jika seluruh asisten sudah ditugaskan, dialog menampilkan penjelasan bahwa belum ada asisten tersedia.

Tombol **Tambah asisten** membuka formulir akun dari dialog kelompok. Setelah akun Asisten baru disimpan, pilihan pembimbing dimuat ulang dan akun tersebut langsung dipilih. Nama serta nomor kelompok yang sudah diisi tetap dipertahankan. Pengguna juga dapat memilih **Tanpa asisten** untuk menyimpan kelompok sebelum pembimbing ditentukan.

Halaman modul menyediakan tab Semua, Berjalan, Selesai, dan Belum Dimulai. Tabel menampilkan kode, judul, kategori, jadwal, bobot, serta progres pengumpulan. Tombol **Nilai** membuka halaman penilaian dengan modul terkait. Admin dapat menambah dan mengubah modul.

Modul yang ditambahkan berlaku untuk seluruh kelompok. Belum ada pilihan kelompok penerima modul. Layanan penghapusan modul tersedia pada lapisan data, tetapi tombol penghapusannya belum tersedia di antarmuka saat laporan ini disusun.

<!-- GAMBAR 07 -->
> **Gambar 7. Halaman kelompok praktikum**  
> [Sisipkan tangkapan layar daftar kelompok di sini.]

<!-- GAMBAR 08 -->
> **Gambar 8. Dialog tambah kelompok dan pemilihan asisten**  
> [Sisipkan tangkapan layar pilihan asisten, keadaan tanpa asisten tersedia, atau alur tambah asisten di sini.]

<!-- GAMBAR 09 -->
> **Gambar 9. Halaman modul praktikum**  
> [Sisipkan tangkapan layar daftar modul dan tab status di sini.]

### 7.5 Pengumpulan dan penilaian

Pengguna memilih modul dan kelompok, kemudian mengisi pengumpulan melalui tabel praktikan. Waktu dapat dimasukkan dengan format `dd/MM/yyyy HH:mm` atau format ISO yang didukung. Status pengumpulan mengikuti perbandingan waktu kumpul dengan tenggat modul.

Tautan tugas menerima URL HTTP atau HTTPS dan dapat dibuka melalui tombol pada baris. Nilai dari 0 sampai 100 serta catatan evaluasi dapat diisi setelah waktu pengumpulan tercatat. Nama penilai ditampilkan pada tabel.

Tombol penandaan tersedia untuk satu baris maupun seluruh baris. Penandaan mengisi waktu komputer pada baris yang belum mempunyai waktu pengumpulan. Perubahan belum disimpan ditandai dan dihitung jumlahnya.

Tombol **Simpan perubahan** menyimpan baris yang berubah dalam satu transaksi. Jika salah satu baris tidak valid, batch ditolak. Tombol **Batalkan perubahan** mengembalikan masukan ke nilai awal. Navigasi ke halaman lain dan logout meminta konfirmasi ketika terdapat perubahan penilaian yang belum disimpan.

Kolom nama, waktu pengumpulan, status, dan nilai ditempatkan sebelum tautan serta catatan. Tabel dapat digeser ke kanan untuk menjangkau kolom lainnya. Perlindungan perubahan ketika pengguna mengganti pilihan modul atau kelompok masih perlu ditambahkan.

<!-- GAMBAR 10 -->
> **Gambar 10. Halaman pengumpulan dan penilaian**  
> [Sisipkan tangkapan layar tabel penilaian, status, dan indikator perubahan di sini.]

### 7.6 Praktikan berisiko

Halaman ini menampilkan jumlah praktikan yang memenuhi kriteria R1, R2, dan R3. Tabel memuat praktikan, kelompok, asisten, jumlah modul belum dikumpulkan, ketuntasan, nilai akhir, serta alasan risikonya.

Satu praktikan dapat memenuhi beberapa kriteria sekaligus. Lencana pada sidebar menunjukkan jumlah praktikan berisiko, bukan jumlah seluruh alasan risiko.

<!-- GAMBAR 11 -->
> **Gambar 11. Daftar praktikan berisiko**  
> [Sisipkan tangkapan layar ringkasan R1, R2, R3 dan daftar praktikan di sini.]

### 7.7 Log aktivitas dan pengguna

Halaman log aktivitas tersedia pada menu Admin. Antarmuka menyediakan pencarian kata kunci, filter jenis aksi, dan pemuatan ulang log. Layanan data juga mendukung parameter pengguna. Filter rentang tanggal belum tersedia pada antarmuka versi ini.

Log menyimpan pelaku, waktu, aksi, entitas, dan keterangan. Perubahan nilai mencatat nilai sebelumnya serta nilai baru. Pencatatan tersebut dilakukan melalui aplikasi dan tidak otomatis mencatat perubahan langsung pada berkas SQLite di luar aplikasi.

Admin dapat menambah serta mengubah akun, mengganti password, menetapkan peran, dan mengubah status aktif. Admin tidak dapat menonaktifkan akun sendiri atau mengubah perannya sendiri. Asisten yang dinonaktifkan atau diubah menjadi Admin dilepas dari penugasan pembimbing kelompok.

<!-- GAMBAR 12 -->
> **Gambar 12. Log aktivitas**  
> [Sisipkan tangkapan layar log perubahan nilai atau aktivitas lainnya di sini.]

<!-- GAMBAR 13 -->
> **Gambar 13. Manajemen pengguna**  
> [Sisipkan tangkapan layar daftar pengguna atau formulir akun di sini.]

### 7.8 Pengaturan dan tema

Pengaturan menyediakan pemilihan tema terang atau gelap, perubahan password dengan indikator kekuatan, nama program, lokasi database, dan pengaturan ambang risiko. Pengubahan ambang risiko serta nama program dibatasi untuk Admin.

Tema dapat berubah saat aplikasi digunakan. Preferensi tema tersimpan pada akun pengguna dan preferensi lokal. Password baru minimal enam karakter dan perubahan password sendiri memerlukan verifikasi password lama.

Admin juga dapat memulihkan data contoh melalui dialog konfirmasi. Tindakan tersebut menghapus perubahan pada database lokal dan membuat kembali data awal. Pemulihan data contoh bukan pemulihan cadangan pengguna.

<!-- GAMBAR 14 -->
> **Gambar 14. Pengaturan dan tema gelap**  
> [Sisipkan tangkapan layar pengaturan dengan tema gelap di sini.]

### 7.9 Notifikasi tindakan

Notifikasi hasil tindakan ditampilkan melalui toast pada jendela utama. Toast hilang otomatis setelah 2 detik. Apabila pesan baru muncul sebelum waktu tersebut habis, timer dimulai ulang sehingga pesan terbaru mendapatkan waktu tampil penuh.

Pengguna dapat menutup toast melalui tombol tutup. Dialog konfirmasi tetap menunggu jawaban pengguna karena memiliki fungsi yang berbeda dari notifikasi sementara.

## 8. Alur data dan penyimpanan

Koordinator menyiapkan akun, kelompok, praktikan, dan modul. Asisten mencatat waktu pengumpulan serta tautan tugas, kemudian memberi nilai. Layanan menyimpan perubahan dan log. Aplikasi membaca kembali rekap untuk memperbarui daftar praktikan, detail, kelompok, dashboard, dan indikator risiko.

Database disimpan di direktori data aplikasi pengguna. Lokasi umumnya adalah:

| Sistem operasi | Lokasi database |
| --- | --- |
| Linux | `~/.local/share/SIMPRAK/simprak.db` |
| Windows | `%LOCALAPPDATA%\SIMPRAK\simprak.db` |

Saat database belum tersedia, aplikasi menjalankan skrip `simprak.sql` pada berkas sementara. Setelah proses berhasil, berkas dipindahkan ke lokasi database akhir. Cara ini menghindari penggunaan database yang baru terbentuk sebagian apabila proses inisialisasi gagal. Foreign key diaktifkan pada koneksi SQLite.

Operasi baca dan tulis utama pada layanan saat ini berjalan secara sinkron. Penilaian batch menggunakan transaksi agar pembaruan seluruh baris dan log terkait berhasil atau dibatalkan bersama. Jaminan transaksi tersebut tidak boleh disamakan dengan seluruh operasi aplikasi, karena beberapa operasi pembuatan data dan log menggunakan penyimpanan terpisah.

Preferensi lokal seperti username yang diingat dan tema terakhir tersimpan pada `preferensi.json` di direktori yang sama dengan database. Sesi login berada di memori dan tidak dipulihkan sebagai login otomatis setelah aplikasi dibuka kembali.

SIMPRAK hanya menyimpan tautan tugas. Berkas tugas tetap berada di layanan atau lokasi eksternal. Database belum dibagikan otomatis antarperangkat dan belum memiliki pencadangan otomatis.

Perhitungan status modul dan jatuh tempo menggunakan pengaturan `waktu_acuan`. Data contoh mengisinya dengan `2026-10-21 23:59`. Karena itu, hasil laporan belum otomatis bergerak mengikuti waktu komputer, meskipun waktu login, pencatatan pengumpulan melalui pintasan, dan audit menggunakan waktu komputer.

## 9. Pengujian

Pengujian otomatis menggunakan xUnit. Layanan data diuji dengan SQLite in-memory yang diinisialisasi dari `simprak.sql`, sehingga constraint, trigger, dan view ikut diperiksa. Pengujian perilaku toast serta alur dialog kelompok menggunakan Avalonia.Headless.

Hasil pengujian terakhir selama pengembangan adalah 48 kasus lulus, terdiri atas 46 kasus aturan bisnis dan layanan data, satu kasus perilaku notifikasi, serta satu kasus alur kelompok dan penambahan asisten.

Cakupan pengujian meliputi:

- Status pengumpulan sebelum, tepat pada, dan setelah tenggat.
- Status modul, ketuntasan, pembulatan, serta nilai akhir berbobot.
- Kriteria risiko dan perubahan ambang oleh Admin.
- Kesesuaian perhitungan C# dengan view SQL pada 124 praktikan data contoh.
- Login, akun tidak valid, serta sejumlah pembatasan akses asisten.
- Penolakan nilai tanpa pengumpulan dan pencatatan perubahan nilai.
- Validasi, penyimpanan, serta pengosongan tautan tugas.
- Validasi NRP, kelompok, tanggal modul, dan perlindungan penghapusan.
- Peningkatan database lama dengan kolom tautan.
- Ekspor CSV untuk pemisah regional koma dan titik koma.
- Toast yang hilang otomatis, waktu ulang pesan baru, dan penutupan manual.
- Penambahan asisten dari dialog kelompok, pemuatan ulang pilihan, pemilihan akun baru, serta pemeliharaan isian ketika dialog dibatalkan.

```bash
dotnet test tests/Simprak.Tests/Simprak.Tests.csproj
```

Ringkasan hasil pengujian terakhir:

```text
Passed! - Failed: 0, Passed: 48, Skipped: 0, Total: 48
```

<!-- GAMBAR 15 -->
> **Gambar 15. Hasil pengujian otomatis**  
> [Sisipkan tangkapan layar hasil `dotnet test` milik sendiri di sini.]

Pemeriksaan visual selama pengembangan mencakup tema terang dan gelap, ukuran jendela 1420 × 960 serta 1280 × 740, formulir, tabel, grafik, dan pengguliran kolom penilaian. Pemeriksaan ini menggunakan rendering headless dan tidak menggantikan pengujian langsung pada seluruh perangkat pengguna.

PRD menyediakan 19 skenario uji penerimaan untuk login, hak akses, pengelolaan data, perhitungan, penyimpanan, ekspor, notifikasi, dan persistensi. Seluruh skenario tersebut belum dinyatakan selesai hanya berdasarkan kelulusan 48 kasus otomatis. Pengujian kegunaan dengan asisten laboratorium sungguhan, uji beban data besar, serta pengujian paket Windows belum tercatat selesai.

## 10. Hasil dan catatan pengembangan

Implementasi SIMPRAK memiliki satu layar login, sepuluh halaman utama, dan lima jenis dialog. Halaman utama mencakup Dashboard, Praktikan, Detail Praktikan, Kelompok, Modul, Penilaian, Berisiko, Log Aktivitas, Pengguna, dan Pengaturan.

Fitur yang sudah dapat digunakan meliputi pengelolaan data, pencatatan pengumpulan serta tautan, penilaian batch, rekap, deteksi risiko, log aktivitas, ekspor CSV, dan tema terang serta gelap. Perbaikan selama pengembangan juga mencakup ukuran teks, kontras, jarak komponen, label filter, pengguliran tabel, dan tooltip.

Beberapa catatan pengembangan adalah:

1. Konsistensi perhitungan perlu dijaga karena rumus terdapat pada C# dan view SQL. Tes membandingkan hasil ketuntasan, nilai akhir, dan risiko pada data contoh.
2. ViewModel per baris penilaian melacak perubahan. Halaman menampilkan jumlah baris yang belum disimpan dan menyediakan tindakan simpan serta batal.
3. Tabel lebar menggunakan scrollbar agar kolom tidak diperkecil sampai sulit dibaca. Kolom nilai ditempatkan sebelum tautan dan catatan agar tindakan utama lebih mudah dijangkau.
4. Timer bawaan Avalonia menutup toast setelah 2 detik. Timer dihentikan dan dimulai ulang saat pesan baru ditampilkan agar pesan lama tidak menutup pesan terbaru terlalu cepat.
5. Daftar asisten kosong pada data awal disebabkan seluruh 12 asisten aktif sudah memiliki kelompok. Dialog sekarang menjelaskan kondisi tersebut, menyediakan penambahan akun, dan membolehkan kelompok disimpan tanpa asisten.
6. Query pilihan asisten diperbaiki agar pembimbing yang sedang digunakan tetap dapat dimuat ketika kelompok diedit.
7. Dukungan tautan tugas mencakup model, skema, validasi, ViewModel, tampilan, dan pengujian. Database lama dapat menerima kolom tambahan tanpa diisi ulang.

Proyek menyediakan konfigurasi penerbitan untuk aplikasi desktop. Berkas distribusi Windows atau Linux, ukuran unduhan, dan tautan unduhan belum dicantumkan karena perlu diisi berdasarkan paket yang benar-benar diterbitkan dan diuji.

## 11. Batasan dan rencana berikutnya

### 11.1 Batasan versi ini

- Database tersimpan pada satu perangkat dan belum disinkronkan antarperangkat.
- Praktikan belum mempunyai akun untuk melihat modul atau mengirim tautan secara mandiri.
- Berkas tugas berada di luar SIMPRAK. Aplikasi hanya menyimpan tautannya.
- Seluruh modul berlaku untuk semua kelompok dan mata kuliah dalam database.
- Perhitungan jatuh tempo menggunakan waktu acuan yang tetap pada data contoh.
- Sebagian ringkasan dashboard dan ambang peringatan grafik masih menggunakan angka tetap.
- Perlindungan perubahan belum disimpan sudah tersedia untuk navigasi serta logout, tetapi belum lengkap ketika pilihan modul atau kelompok diganti.
- Ketika filter tidak menghasilkan praktikan, ekspor saat ini kembali menggunakan seluruh daftar yang dimuat. Perilaku ini perlu disesuaikan agar ekspor mengikuti filter tanpa kejutan.
- Pemeriksaan akses pada layanan perlu diperkuat, terutama untuk asisten yang belum mempunyai kelompok, pembacaan administratif, dan perubahan kelompok asal praktikan.
- Penghapusan modul belum tersedia pada antarmuka. Layanan penghapusannya menghapus pengumpulan terkait melalui relasi database.
- Pembacaan dan penulisan utama masih sinkron. Pengaruhnya pada respons antarmuka memerlukan pengujian dengan data lebih besar.
- Pencadangan otomatis, pemulihan cadangan melalui aplikasi, serta pemisahan semester belum tersedia.
- Kelulusan pengujian pada data contoh belum menjadi bukti keberhasilan seluruh UAT atau penggunaan pada program praktikum nyata.

Pemeriksaan dependensi juga masih melaporkan peringatan keamanan pada paket SQLite dan Tmds.DBus.Protocol yang digunakan secara transitif. Pembaruan paket beserta pengujian kompatibilitas menjadi tindak lanjut teknis.

### 11.2 Rencana pengembangan

Prioritas perbaikan adalah menutup kesenjangan yang memengaruhi akses, kehilangan perubahan, dan ketepatan laporan. Pekerjaan tersebut meliputi penguatan otorisasi layanan, konfirmasi pergantian modul atau kelompok, perbaikan ekspor tanpa hasil, penghapusan angka tetap pada dashboard, serta penetapan sumber waktu operasional.

Pengembangan berikutnya dapat mencakup:

- Penetapan modul untuk kelompok atau mata kuliah tertentu.
- Pengelolaan periode atau semester sebagai data terpisah.
- Pencadangan dan pemulihan database dari aplikasi.
- Sinkronisasi data atau layanan terpusat untuk penggunaan pada beberapa komputer.
- Portal praktikan untuk melihat modul dan mengirim tautan pengumpulan.
- Pengingat tenggat, pencatatan presensi, serta laporan PDF.
- Pengujian kegunaan, kinerja pada data lebih besar, dan paket distribusi pada kedua sistem operasi.

Daftar tersebut merupakan usulan pengembangan. Ruang lingkup dan prioritasnya perlu ditetapkan sebelum implementasi.

## 12. Kesimpulan

SIMPRAK menyatukan pengelolaan praktikan, kelompok, modul, pengumpulan, dan penilaian dalam aplikasi desktop. Aturan status pengumpulan, ketuntasan, nilai akhir berbobot, serta risiko memberikan dasar perhitungan yang sama bagi rekap. Log aktivitas membantu menelusuri tindakan dan perubahan nilai.

Pemisahan antarmuka, aturan inti, dan akses data mendukung pengujian perhitungan tanpa membuka aplikasi desktop. Hasil pengujian terakhir menunjukkan 48 kasus otomatis lulus, termasuk perbaikan notifikasi dan alur penambahan asisten dari formulir kelompok.

Versi saat ini berfokus pada pencatatan lokal pada satu komputer. Penguatan hak akses, konsistensi waktu dan dashboard, perlindungan perubahan, serta pengujian operasional masih diperlukan. Pemetaan modul per kelompok, sinkronisasi antarperangkat, portal praktikan, dan pencadangan otomatis menjadi arah pengembangan berikutnya.
