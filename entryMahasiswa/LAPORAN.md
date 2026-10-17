# Membuat UI Sederhana dari Sistem Registrasi Mahasiswa dengan Menggunakan WPF

## Tata Cara Menjalankan Aplikasi

Aplikasi ini menggunakan **WPF .NET 8**, sehingga perlu dijalankan di Windows dengan .NET 8 SDK (atau Visual Studio 2022 dengan workload **.NET desktop development**).

### Menjalankan melalui terminal

1. Buka PowerShell atau terminal pada folder `entryMahasiswa`.
2. Pastikan .NET 8 SDK terpasang dengan menjalankan `dotnet --version`.
3. Pulihkan dependency dan jalankan aplikasi:

   ```powershell
   dotnet restore .\StudentRegistrationApp.csproj
   dotnet run --project .\StudentRegistrationApp.csproj
   ```

   Perintah tersebut juga dapat dijalankan dari folder induk menggunakan path `entryMahasiswa\StudentRegistrationApp.csproj`.

### Menjalankan melalui Visual Studio

1. Buka `StudentRegistrationApp.csproj` menggunakan Visual Studio 2022 di Windows.
2. Pastikan workload **.NET desktop development** sudah dipasang.
3. Pilih konfigurasi `Debug` dan tekan **F5** (atau **Ctrl+F5** untuk menjalankan tanpa debugger).

Data aplikasi disimpan di memori, sehingga akan dihapus saat aplikasi ditutup. Database belum digunakan karena soal menempatkan database sebagai pengembangan lanjutan.

---

**Source Code Github (referensi format laporan):** https://github.com/andrawpt/fbp-course/tree/main/Exc-4

## Gambaran Umum Sistem

Aplikasi ini merupakan aplikasi desktop registrasi mahasiswa berbasis WPF. Antarmuka dibuat menggunakan XAML, sedangkan validasi dan pengelolaan data ditangani oleh C#. Fitur yang tersedia:

- Menambahkan dan menampilkan data mahasiswa.
- Memvalidasi NIM, nama, program studi, dan jenis kelamin.
- Mencegah NIM yang sama digunakan lebih dari satu kali.
- Mencari data berdasarkan NIM, nama, program studi, atau jenis kelamin.
- Memilih data mahasiswa untuk mengisi form dan memperbaruinya.
- Menghapus data terpilih dengan konfirmasi.
- Mereset form dan menampilkan jumlah mahasiswa.

Kontrol yang digunakan memenuhi kebutuhan soal, termasuk `TextBox`, `ComboBox`, `RadioButton`, `Button`, dan `ListBox`. Event utama yang digunakan adalah `Click`, `TextChanged`, dan `SelectionChanged`.

## Struktur Folder

```text
entryMahasiswa/
├── App.xaml
├── App.xaml.cs
├── LAPORAN.md
├── Mahasiswa.cs
├── MainWindow.xaml
├── MainWindow.xaml.cs
└── StudentRegistrationApp.csproj
```

## Penjelasan File

### 1. `StudentRegistrationApp.csproj`

File project mengatur aplikasi sebagai executable Windows dan mengaktifkan WPF. Target framework yang digunakan adalah `net8.0-windows`; `UseWPF` mengaktifkan kompilasi XAML serta referensi kontrol WPF. Nullable reference types dan implicit usings juga diaktifkan.

### 2. `App.xaml` dan `App.xaml.cs`

`App.xaml` mendeklarasikan resource tampilan bersama seperti warna utama, warna latar, serta style dasar untuk `TextBox`, `ComboBox`, dan `Button`. Atribut `StartupUri="MainWindow.xaml"` menjadikan window utama sebagai tampilan pertama aplikasi.

`App.xaml.cs` berisi class aplikasi WPF (`App`), yang menjadi titik masuk siklus hidup aplikasi. Tidak diperlukan kode inisialisasi khusus karena konfigurasi awal sudah ditentukan di XAML.

### 3. `Mahasiswa.cs`

Class `Mahasiswa` merupakan model data untuk satu mahasiswa. Properti `Nim`, `Nama`, `Prodi`, dan `JenisKelamin` menyimpan nilai dari form. Implementasi `INotifyPropertyChanged` memberi tahu antarmuka ketika properti berubah, sehingga perubahan data terpilih bisa langsung diperbarui pada daftar.

- `Inisial` menghasilkan inisial nama untuk avatar pada item daftar.
- `Detail` menggabungkan program studi dan jenis kelamin untuk informasi sekunder.

### 4. `MainWindow.xaml`

File ini menyusun tampilan aplikasi: header, ringkasan jumlah mahasiswa, form input, kolom pencarian, dan daftar mahasiswa. Kontrol-kontrol diberi nama (`x:Name`) agar dapat diakses oleh kode C#.

- **TextBox**: input NIM/nama serta kolom pencarian.
- **ComboBox**: memilih program studi.
- **RadioButton**: memilih jenis kelamin.
- **Button**: simpan, perbarui, reset, dan hapus.
- **ListBox**: menampilkan daftar mahasiswa dengan template item.

Atribut `Click`, `TextChanged`, dan `SelectionChanged` menghubungkan interaksi antarmuka dengan handler di `MainWindow.xaml.cs`.

### 5. `MainWindow.xaml.cs`

Window menyimpan data aktif dalam `ObservableCollection<Mahasiswa>`. Koleksi ini dihubungkan ke `ListBox` melalui `ICollectionView`, yang juga menyediakan filter pencarian.

- `BtnSimpan_Click` memvalidasi form dan menambahkan mahasiswa baru. NIM duplikat ditolak.
- `BtnPerbarui_Click` memperbarui mahasiswa yang dipilih, dengan validasi NIM duplikat.
- `BtnHapus_Click` meminta konfirmasi sebelum menghapus data terpilih.
- `BtnReset_Click` dan `ResetForm` mengosongkan form.
- `TxtCari_TextChanged` memfilter data secara langsung berdasarkan NIM, nama, program studi, atau jenis kelamin.
- `LstMahasiswa_SelectionChanged` memindahkan data yang dipilih ke form.
- `TryReadForm` memeriksa bahwa semua input wajib sudah diisi.
- `UpdateTotal` memperbarui jumlah mahasiswa di kartu ringkasan.

## Alur Penggunaan

1. Isi NIM dan nama mahasiswa.
2. Pilih program studi dan jenis kelamin.
3. Tekan **Simpan Mahasiswa**. Data yang valid akan muncul pada daftar.
4. Gunakan kolom pencarian untuk menyaring daftar.
5. Pilih item dari daftar agar datanya dimuat ke form, lalu tekan **Perbarui** atau **Hapus Data Terpilih**.
6. Tekan **Reset** untuk mengosongkan form.

## Pengujian yang Disarankan

| Skenario | Hasil yang diharapkan |
| --- | --- |
| Simpan tanpa mengisi NIM | Pesan validasi NIM muncul |
| NIM terisi, nama kosong | Pesan validasi nama muncul |
| Program studi atau jenis kelamin belum dipilih | Pesan validasi pilihan muncul |
| Simpan seluruh input valid | Data muncul di daftar dan total bertambah |
| Simpan NIM yang sudah terdaftar | Data ditolak dan pesan NIM duplikat muncul |
| Cari sebagian NIM atau nama | Daftar menampilkan data yang cocok |
| Pilih data, ubah nilai, lalu perbarui | Data daftar berubah |
| Pilih data lalu hapus dan jawab Ya | Data dihapus dan total berkurang |
| Tekan Reset | Seluruh input kembali kosong |

## Batasan

Data saat ini hanya tersimpan selama aplikasi berjalan. Pengembangan berikutnya dapat menambahkan persistensi SQLite/SQL Server, serta field tanggal lahir, alamat, dan nomor telepon seperti tantangan pada soal.
