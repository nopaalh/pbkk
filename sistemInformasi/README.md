# SIMPRAK — Sistem Informasi Monitoring dan Penilaian Praktikum

> **Tagline:** *Setiap progres praktikan, terpantau akurat.*

Dokumen proyek: [PRD](PRD.md) · [Laporan pengembangan](LAPORAN.md).

---

## 1. Ringkasan Proyek
**SIMPRAK (Sistem Informasi Monitoring dan Penilaian Praktikum)** membantu Koordinator Laboratorium (Admin) dan Asisten Laboratorium memantau kegiatan praktikum perkuliahan. Aplikasi menyatukan data praktikan, kelompok shift praktikum, modul penugasan, pengumpulan tautan tugas, dan penilaian dalam satu tempat terpadu. Status pengumpulan, ketuntasan, nilai akhir berbobot, dan indikator praktikan rawan mengulang (berisiko) dihitung dari sumber data yang sama, sehingga rekapitulasi nilai tidak perlu digabungkan manual dari banyak *spreadsheet*.

SIMPRAK dibangun sebagai aplikasi desktop lintas platform (Windows & Linux) dengan **C# 12**, **.NET**, dan **Avalonia UI (XAML)**. Aplikasi menggunakan pola **Model–View–ViewModel (MVVM)**, **Entity Framework Core**, dan **SQLite**. Seluruh data tersimpan secara lokal di satu komputer (`~/.local/share/SIMPRAK/simprak.db` pada Linux atau `%LOCALAPPDATA%\SIMPRAK\simprak.db` pada Windows).

---

## 2. Latar Belakang dan Tujuan
Dalam penyelenggaraan praktikum laboratorium, praktikan dibagi ke dalam sejumlah kelompok shift yang dibimbing oleh asisten laboratorium dan menerima beberapa modul penugasan selama satu semester. Asisten mencatat pengumpulan serta memberi nilai, sedangkan Koordinator Lab perlu memantau perkembangan seluruh kelompok agar dapat membantu praktikan yang tertinggal.

Jika setiap asisten memakai *spreadsheet* dengan format berbeda, data harus disatukan terlebih dahulu sebelum bisa diperiksa. Status terlambat pun rentan ditentukan dengan cara yang tidak konsisten. Perubahan nilai sulit ditelusuri, dan berkas bersama tidak selalu membatasi siapa yang boleh mengubah data kelompok tertentu.

**SIMPRAK dikembangkan untuk:**
* mengelola akun pengguna (Koordinator/Admin dan Asisten Lab), kelompok praktikum, praktikan, dan modul penugasan;
* mencatat waktu pengumpulan dan tautan hasil tugas (`http://` atau `https://`), lalu menentukan statusnya dari tenggat secara otomatis;
* menyimpan nilai dan catatan penilaian, termasuk pembaruan *batch* untuk beberapa praktikan sekaligus dalam satu transaksi;
* menghitung ketuntasan modul dan nilai akhir rata-rata berbobot;
* menandai praktikan yang memenuhi kriteria risiko rawan mengulang (`R1`, `R2`, `R3`) yang dapat diatur ambangnya oleh Admin;
* menampilkan ringkasan melalui *dashboard*, grafik batang & garis kustom (`OnRender`), tabel interaktif, dan aktivitas terbaru;
* membatasi akses asisten hanya ke kelompok praktikum yang dibimbingnya;
* menyimpan jejak audit perubahan, termasuk nilai sebelumnya dan nilai baru saat terjadi perubahan nilai; serta
* menyediakan tema terang dan gelap dengan palet warna hijau khas **180 Degrees Consulting (`#6D9E31` / `#8BC34A`)** serta ekspor CSV.

---

## 3. Kebutuhan dan Aturan Bisnis
Dokumen [PRD SIMPRAK](PRD.md) memuat ruang lingkup, peran dan hak akses, 41 kebutuhan fungsional, kebutuhan nonfungsional, aturan bisnis, rancangan layar, arsitektur, serta skenario uji penerimaan. Dokumen tersebut juga mencatat kebutuhan notifikasi 2 detik, keterbacaan antarmuka, dan kesenjangan implementasi yang masih perlu ditangani.

**Aturan bisnis utama yang digunakan aplikasi:**
* **BR-01 — Status pengumpulan:** *Tepat Waktu* jika waktu pengumpulan tidak melewati tenggat; *Terlambat* jika melewati tenggat; *Belum Mengumpulkan* jika belum ada catatan pengumpulan.
* **BR-02 — Proteksi integritas data:** Praktikan yang sudah memiliki riwayat pengumpulan/nilai tidak dapat dihapus. Kelompok yang masih memiliki anggota tidak dapat dihapus. Admin tidak dapat menonaktifkan akun sendiri atau mengubah perannya sendiri.
* **BR-03 — Ketuntasan:** jumlah modul jatuh tempo yang dikumpulkan dibagi jumlah modul jatuh tempo (dibulatkan 2 desimal).
* **BR-04 — Nilai akhir:** rata-rata berbobot berdasarkan bobot tiap modul (`1–100`). Modul jatuh tempo yang tidak dikumpulkan dihitung bernilai `0`. Pengumpulan yang sudah masuk namun belum dinilai tidak masuk ke perhitungan sampai dinilai.
* **BR-05 — Praktikan berisiko:** praktikan memenuhi kriteria jika sedikitnya satu kondisi berikut tercapai:
  * **R1:** dua atau lebih modul jatuh tempo belum dikumpulkan;
  * **R2:** ketuntasan di bawah ambang (nilai awal `75%`); atau
  * **R3:** nilai akhir di bawah ambang (nilai awal `60`) dengan sedikitnya dua modul sudah dinilai.

---

## 4. Desain Antarmuka & Palet Warna (180DC Green Edition)

| Elemen | Tema Terang | Tema Gelap | Penggunaan |
| :--- | :--- | :--- | :--- |
| **Aksen** | `#6D9E31` | `#8BC34A` | Tombol utama, menu aktif, tautan, grafik |
| **Sidebar** | `#111C14` | `#09100B` | Navigasi samping (*Deep Forest Charcoal*) |
| **Latar** | `#F4F7F2` | `#0D1510` | Latar halaman utama |
| **Permukaan** | `#FFFFFF` | `#152219` | Kartu statistik, tabel, dan dialog |
| **Berhasil** | `#15803D` | `#4ADE80` | *Tepat Waktu*, *Aman*, dan *Aktif* |
| **Peringatan** | `#B45309` | `#FBBF24` | *Terlambat* dan ketuntasan kelompok di bawah 75% |
| **Bahaya** | `#B91C1C` | `#F87171` | *Belum Mengumpulkan*, *Praktikan Berisiko*, dan galat |
| **Informasi** | `#0369A1` | `#38BDF8` | *Belum Dinilai* dan informasi umum |

---

## 5. Struktur Proyek

```text
SIMPRAK/
├── Simprak.slnx
├── README.md
├── database/
│   ├── simprak.sql              # Skema 7 tabel, 9 indeks, 7 trigger, 6 view, dan 124 seed data
│   └── PDM_SIMPRAK.png          # Diagram Physical Data Model
├── src/
│   ├── Simprak.Core/            # Model dan aturan bisnis (BR-01..BR-05) tanpa dependensi UI
│   │   ├── Models/
│   │   ├── Rules/
│   │   └── Services/
│   ├── Simprak.Data/            # SQLite, EF Core, migrasi skema, dan layanan data
│   │   ├── SimprakDbContext.cs
│   │   ├── DatabaseInitializer.cs
│   │   └── Services/
│   └── Simprak.App/             # Aplikasi Desktop Avalonia UI (XAML)
│       ├── Views/
│       ├── ViewModels/
│       ├── Controls/            # SimprakBarChart & SimprakLineChart (OnRender)
│       ├── Converters/
│       └── Services/
└── tests/
    └── Simprak.Tests/           # 46 unit & integration test otomatis xUnit
```

---

## 6. Cara Menjalankan Aplikasi & Pengujian

### Menjalankan Aplikasi Desktop
```bash
dotnet run --project src/Simprak.App/Simprak.App.csproj
```

**Akun Demo:**
* **Koordinator Lab (Admin):** Username `admin` | Password `admin123`
* **Asisten Lab (Shift A1):** Username `asisten01` | Password `asisten123`

### Menjalankan 46 Unit Test xUnit
```bash
dotnet test tests/Simprak.Tests/Simprak.Tests.csproj
```
*(Seluruh 46 pengujian otomatis mencakup verifikasi aturan bisnis `BR-01` s.d. `BR-05`, kecocokan 100% antara perhitungan C# dan View SQLite pada 124 praktikan, otorisasi asisten, audit perubahan nilai, validasi tautan `FR-39`, migrasi skema database lama, serta ekspor CSV).*

### Mempaketkan Berkas Mandiri (.exe Windows / Binary Linux)
```bash
# Untuk Windows (.exe self-contained):
dotnet publish src/Simprak.App/Simprak.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# Untuk Linux (binary self-contained):
dotnet publish src/Simprak.App/Simprak.App.csproj -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```
