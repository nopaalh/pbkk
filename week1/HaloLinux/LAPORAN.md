# 📘 Laporan Praktikum PBKK - Week 1: Sistem Data Mahasiswa Berbasis Console (.NET)

---

## 📌 Metadata Laporan
* **Mata Kuliah:** Pemrograman Berbasis Kerangka Kerja (PBKK)
* **Topik:** Pengenalan .NET Core / .NET Console Application & C# OOP Fundamentals
* **Bahasa / Framework:** C# 13 / .NET 10.0 (net10.0)
* **Platform Target:** Linux / Cross-Platform
* **Repository Folder:** `week1/HaloLinux`

---

## 1. 🎯 Gambaran Umum Proyek
Proyek ini merupakan aplikasi konsol interaktif berbasis .NET yang berfungsi sebagai **Sistem Manajemen Data Mahasiswa** sederhana. Aplikasi ini mengimplementasikan konsep dasar pemrograman berorientasi objek (OOP), manipulasi struktur data di memori (*in-memory list*), validasi input terminal, operasi CRUD (Create, Read, Update, Delete), serta perataan output teks tabular (*formatted string table*).

### Fitur Utama:
1. **Tambah Data Mahasiswa (Create):** Menginputkan NIM, Nama Lengkap, Program Studi, dan IPK dengan konversi angka desimal valid.
2. **Tampilkan Semua Mahasiswa (Read All):** Menyajikan seluruh entri dalam format tabel rapi dengan kolom sejajar.
3. **Cari Mahasiswa Berdasarkan NIM (Read Single):** Menggunakan ekspresi LINQ untuk menemukan data mahasiswa secara spesifik.
4. **Hapus Mahasiswa Berdasarkan NIM (Delete):** Menghapus data terpilih serta menangani kasus jika NIM tidak terdaftar.
5. **Keluar Sistem (Exit):** Menghentikan jalannya loop aplikasi secara aman.

---

## 2. 📂 Struktur Proyek & Analisis Direktori

```text
week1/HaloLinux/
├── bin/                          # Folder output biner yang siap dieksekusi
│   └── Debug/net10.0/
│       ├── HaloLinux             # AppHost native executable (Linux ELF binary)
│       ├── HaloLinux.dll         # Managed Assembly biner (IL Code)
│       ├── HaloLinux.pdb         # Simbol debug untuk pelacakan error
│       ├── HaloLinux.deps.json   # Metadata pemetaan dependensi
│       └── HaloLinux.runtimeconfig.json  # Konfigurasi runtime CLR
├── obj/                          # Folder intermediate (objek sementara kompilasi)
│   ├── Debug/net10.0/            # File build sementara per-kompilasi
│   ├── project.assets.json       # Resolusi paket dan manifest NuGet
│   └── *.nuget.g.props / targets # File konfigurasi MSBuild otomatis
├── HaloLinux.csproj              # File konfigurasi XML project .NET
└── Program.cs                    # Titik masuk utama (Entry Point) dan logika program
```

### Penjelasan File Inti:
* **`HaloLinux.csproj`**: Mengonfigurasi SDK (`Microsoft.NET.Sdk`), tipe output sebagai aplikasi mandiri (`<OutputType>Exe</OutputType>`), dan runtime target `<TargetFramework>net10.0</TargetFramework>`.
* **`Program.cs`**: Berisi deklarasi kelas entitas `Mahasiswa`, inisialisasi koleksi `List<Mahasiswa>`, loop kendali aplikasi (*Control Loop*), serta modul-modul fungsi CRUD.

---

## 3. 🧠 Penjelasan Logika & Arsitektur Kode

### A. Model Data Entitas (`Mahasiswa`)
Entitas didefinisikan dengan prinsip enkapsulasi data:
```csharp
class Mahasiswa
{
    public string Nim { get; set; } = string.Empty;
    public string Nama { get; set; } = string.Empty;
    public string Prodi { get; set; } = string.Empty;
    public double Ipk { get; set; }

    public Mahasiswa(string nim, string nama, string prodi, double ipk)
    {
        Nim = nim;
        Nama = nama;
        Prodi = prodi;
        Ipk = ipk;
    }
}
```

### B. Siklus Hidup Aplikasi (*The Control Loop*)
Aplikasi berjalan menggunakan pola **REPL** *(Read-Evaluate-Print-Loop)* di dalam method `Main()`:
* Variabel boolean `berjalan` mengontrol loop `while (berjalan)`.
* `Console.ReadLine()` membaca perintah pilihan menu (1-5).
* Konstruksi `switch-case` mengevaluasi dan mengarahkan alur ke method yang sesuai.

### C. Teknik Perataan Format Tabel
Agar tabel tersusun sejajar secara vertikal terlepas dari panjang teks nama atau prodi, digunakan *string interpolation padding* (`{nilai, -lebar}`):
```csharp
Console.WriteLine($"{"NIM",-15}| {"Nama",-30}| {"Prodi",-20}| IPK");
Console.WriteLine($"{m.Nim,-15}| {m.Nama,-30}| {m.Prodi,-20}| {m.Ipk.ToString("0.00", CultureInfo.InvariantCulture)}");
```
* Tanda minus (`-`) menandakan teks rata kiri (*left-aligned*).
* `CultureInfo.InvariantCulture` memastikan format pecahan desimal konsisten menggunakan titik (`.`), bukan koma.

### D. Operasi Pencarian dan Penghapusan dengan LINQ
Pencarian data dilakukan secara efisien menggunakan method ekstensi LINQ `FirstOrDefault()`:
```csharp
var mhs = daftarMahasiswa.FirstOrDefault(m => m.Nim.Equals(nim, StringComparison.OrdinalIgnoreCase));
```
Kriteria pencarian bersifat *case-insensitive* untuk mencegah kegagalan akibat perbedaan kapitalisasi input.

---

## 4. 🚀 Tata Cara Menjalankan Aplikasi

Pastikan .NET SDK (versi 8.0, 9.0, atau 10.0) telah terpasang pada sistem:

```bash
# 1. Navigasi ke folder project
cd week1/HaloLinux

# 2. Build proyek untuk memastikan tidak ada kesalahan sintaks
dotnet build

# 3. Jalankan aplikasi
dotnet run
```

---

## 5. 🧪 Hasil Pengujian Sistem (Test Cases & Output)

### Skenario 1: Menampilkan Menu Utama
**Tampilan Awal:**
```text
================================
     SISTEM DATA MAHASISWA
================================
1. Tambah Data Mahasiswa
2. Tampilkan Semua Mahasiswa
3. Cari Mahasiswa (berdasarkan NIM)
4. Hapus Mahasiswa (berdasarkan NIM)
5. Keluar
--------------------------------
Pilih menu: 
```

---

### Skenario 2: Menambahkan Data Mahasiswa (Menu 1)
**Input & Output:**
```text
Pilih menu: 1

Tambah Data Mahasiswa
NIM    : 5025241187
Nama   : Kadek Andra Wikanjaya Putra
Prodi  : Teknik Informatika
IPK (0.00 - 4.00): 3.74
Data mahasiswa berhasil ditambahkan.
```

---

### Skenario 3: Menampilkan Seluruh Data Mahasiswa (Menu 2)
**Output Tabel:**
```text
Pilih menu: 2

Daftar Mahasiswa
----------------------------------------------------------------------
NIM            | Nama                          | Prodi               | IPK
----------------------------------------------------------------------
5025241187     | Kadek Andra Wikanjaya Putra   | Teknik Informatika  | 3.74
----------------------------------------------------------------------
```

---

### Skenario 4: Mencari Mahasiswa Berdasarkan NIM (Menu 3)
**Kasus Data Ditemukan:**
```text
Pilih menu: 3

Cari Mahasiswa
Masukkan NIM: 5025241187

Data Ditemukan:
NIM    : 5025241187
Nama   : Kadek Andra Wikanjaya Putra
Prodi  : Teknik Informatika
IPK    : 3.74
```

---

### Skenario 5: Menghapus Mahasiswa Berdasarkan NIM (Menu 4)
**Kasus Berhasil Dihapus:**
```text
Pilih menu: 4

Hapus Mahasiswa
Masukkan NIM: 5025241187
Data mahasiswa dengan NIM 5025241187 berhasil dihapus.
```

**Kasus NIM Tidak Ditemukan:**
```text
Pilih menu: 4

Hapus Mahasiswa
Masukkan NIM: 5025241166
Mahasiswa dengan NIM 5025241166 tidak ditemukan.
```

---

## 6. 💡 Analisis & Kesimpulan

1. **Pemahaman Arsitektur .NET:**
   Melalui praktikum ini, dipahami bahwa file binary di folder `bin/` (`HaloLinux.dll`) dieksekusi oleh runtime CLR melalui wrapper native (`HaloLinux`), sedangkan folder `obj/` menampung artefak intermediate dan hasil caching paket NuGet.
2. **Kesiapan Menuju GUI & Database:**
   Logika bisnis yang dibangun di aplikasi konsol ini (struktur model data dan algoritma CRUD) merupakan fondasi yang dapat langsung dipindahkan ke aplikasi GUI berbasis desktop (WPF / .NET MAUI) maupun Web API (ASP.NET Core) dengan hanya mengganti lapisan tampilan (*presentation layer*).
3. **Penyimpanan Memori Sementara (*In-Memory*):**
   Data saat ini disimpan pada memori volatile RAM via `List<T>`. Untuk pengembangan selanjutnya, persistensi data dapat diintegrasikan dengan database relasional (seperti SQLite atau PostgreSQL) melalui Entity Framework Core.
