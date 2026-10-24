# Product requirements document SIMPRAK

| Informasi | Nilai |
| --- | --- |
| Nama produk | SIMPRAK, Sistem Informasi Monitoring dan Penilaian Praktikum |
| Versi dokumen | 1.0 |
| Tanggal | 22 Oktober 2026 |
| Status | Draf untuk peninjauan |
| Platform | Aplikasi desktop Windows dan Linux |
| Bahasa antarmuka | Bahasa Indonesia |
| Dasar penyusunan | Kode sumber, skema database, pengujian, dan kebutuhan dalam proyek ini |

Dokumen ini menetapkan kebutuhan produk dan kriteria penerimaannya. Fitur yang sudah tersedia menjadi dasar spesifikasi. Perbedaan antara kebutuhan dan implementasi dicatat pada bagian 13. Target kinerja dan persetujuan pengguna belum dianggap tercapai sebelum melalui uji penerimaan.

## 1. Ringkasan produk

SIMPRAK membantu koordinator dan asisten laboratorium mengelola praktikum dalam satu aplikasi. Pengguna mencatat data praktikan, pembagian kelompok, modul, waktu pengumpulan, tautan tugas, nilai, dan catatan evaluasi.

Aplikasi menghitung ketuntasan, nilai akhir berbobot, dan indikator praktikan yang membutuhkan perhatian. Koordinator dapat melihat rekap seluruh kelompok. Asisten menggunakan data kelompok yang dibimbingnya sesuai hak akses.

Data tersimpan pada database SQLite lokal. Pengoperasian fitur inti tidak membutuhkan server atau koneksi internet. Membuka tautan tugas tetap membutuhkan aplikasi atau koneksi yang sesuai dengan alamat tujuan.

## 2. Masalah dan tujuan

### 2.1 Masalah yang ditangani

Pencatatan praktikum melalui berkas terpisah menyulitkan penggabungan nilai, pemeriksaan keterlambatan, dan pemantauan praktikan yang tertinggal. Perubahan nilai juga sulit ditelusuri jika tidak memiliki riwayat.

SIMPRAK menyediakan sumber data yang sama untuk pencatatan pengumpulan, penilaian, dan pelaporan. Sistem menggunakan aturan perhitungan yang konsisten agar pengguna tidak menghitung ulang rekap secara manual.

### 2.2 Tujuan produk

1. Menyatukan pengelolaan praktikan, kelompok, modul, pengumpulan, dan penilaian.
2. Menghasilkan status pengumpulan, ketuntasan, serta nilai berbobot dari data tersimpan.
3. Membantu koordinator menemukan praktikan berisiko melalui alasan yang dapat diperiksa.
4. Membatasi tindakan pengguna berdasarkan peran dan kelompok bimbingan.
5. Menyediakan riwayat perubahan nilai dan ekspor rekap CSV.
6. Menyajikan informasi yang terbaca jelas pada tema terang maupun gelap.

### 2.3 Ukuran keberhasilan

| Ukuran | Kriteria keberhasilan | Cara memeriksa |
| --- | --- | --- |
| Ketepatan perhitungan | Hasil sesuai BR-01 sampai BR-05 dan konsisten antara C# dengan SQL | Uji aturan dan perbandingan hasil pada data contoh |
| Integritas penilaian | Satu batch tersimpan seluruhnya atau tidak tersimpan sama sekali | Masukkan satu baris tidak valid di antara baris valid |
| Pembatasan akses | Asisten tidak dapat membaca atau mengubah data kelompok lain | Uji melalui antarmuka dan pemanggilan layanan data |
| Keterlacakan nilai | Perubahan nilai mencatat pelaku, nilai lama, nilai baru, dan waktu | Simpan perubahan lalu periksa log aktivitas |
| Keterbacaan | Teks utama, formulir, dan tindakan tetap terbaca pada ukuran minimum jendela | Uji visual pada 1280 × 740 dalam kedua tema |
| Notifikasi | Toast hilang otomatis setelah 2 detik, dengan waktu ulang untuk pesan baru | Uji timer dan penggantian pesan |

Ukuran tersebut adalah syarat penerimaan, bukan hasil survei pengguna. Belum ada data pengukuran penghematan waktu kerja atau tingkat adopsi.

## 3. Pengguna dan hak akses

| Pengguna | Kebutuhan utama | Hak akses yang disyaratkan |
| --- | --- | --- |
| Koordinator laboratorium, peran Admin | Memantau seluruh praktikum dan menjaga data induk | Mengelola akun, kelompok, praktikan, modul, penilaian, pengaturan, serta melihat log dan seluruh rekap |
| Asisten laboratorium, peran Asisten | Mencatat pengumpulan dan menilai kelompok bimbingan | Melihat dan mengelola praktikan serta penilaian kelompok sendiri, membaca modul, mengekspor rekap yang diizinkan, mengubah password dan tema sendiri |
| Praktikan | Menjadi subjek pemantauan dan penilaian | Tercatat sebagai data akademik, tanpa akun masuk pada versi ini |

Satu asisten dapat membimbing paling banyak satu kelompok. Kelompok boleh belum memiliki asisten. Asisten yang belum ditugaskan tidak boleh memperoleh akses seluruh kelompok sebagai pengganti penugasan yang kosong.

Pembatasan akses harus berlaku pada layanan data, selain pengaturan visibilitas menu. Matriks ini menjelaskan kebutuhan produk. Celah implementasi yang masih ditemukan dicatat pada bagian 13.

## 4. Ruang lingkup

### 4.1 Termasuk dalam versi ini

- Autentikasi akun lokal dan pembatasan peran.
- Pengelolaan praktikan, kelompok, modul, dan akun pengguna.
- Pencatatan waktu pengumpulan serta tautan tugas.
- Penilaian individual dan penyimpanan beberapa baris dalam satu transaksi.
- Rekap ketuntasan, nilai berbobot, dan deteksi risiko R1, R2, serta R3.
- Dashboard, detail praktikan, grafik, pencarian, filter, dan ekspor CSV.
- Log aktivitas, pengaturan ambang risiko, tema, dan perubahan password.
- Notifikasi otomatis 2 detik dan konfirmasi tindakan yang berpotensi menghilangkan perubahan.
- Penyimpanan lokal, inisialisasi database, dan pemulihan data contoh untuk demonstrasi.

### 4.2 Di luar ruang lingkup

- Portal mahasiswa dan pengumpulan tugas mandiri oleh praktikan.
- Unggah atau penyimpanan berkas tugas. Sistem menyimpan tautan menuju berkas atau repositori eksternal.
- Sinkronisasi antarkomputer, layanan cloud, aplikasi web, dan aplikasi seluler.
- Integrasi sistem akademik, LMS, email, atau pesan instan.
- Impor massal dari spreadsheet, ekspor PDF, dan ekspor Excel dalam format `.xlsx`.
- Pengelolaan semester sebagai entitas terpisah dan pemetaan modul khusus per mata kuliah.
- Pencadangan terjadwal dan pemulihan cadangan pengguna melalui antarmuka.

Pemulihan data contoh menghapus perubahan lokal dan membuat kembali data awal. Fitur tersebut bukan mekanisme pencadangan.

## 5. Alur penggunaan utama

### 5.1 Persiapan oleh koordinator

1. Koordinator masuk menggunakan akun Admin yang aktif.
2. Koordinator membuat akun asisten dan kelompok praktikum.
3. Koordinator menetapkan asisten pembimbing pada kelompok yang sesuai.
4. Koordinator menambahkan praktikan dan menentukan kelompoknya.
5. Koordinator membuat modul, tanggal mulai, tenggat, kategori, dan bobot.
6. Koordinator menetapkan nama program serta ambang risiko.

### 5.2 Pengumpulan dan penilaian

1. Asisten masuk dan membuka halaman **Pengumpulan & Nilai**.
2. Asisten memilih modul dan kelompok yang diizinkan.
3. Asisten mencatat waktu pengumpulan serta tautan tugas jika tersedia.
4. Sistem menentukan status pengumpulan dan mengaktifkan masukan nilai setelah waktu pengumpulan terisi.
5. Asisten mengisi nilai dan catatan evaluasi.
6. Sistem menandai baris yang berubah dan menampilkan jumlah perubahan yang belum disimpan.
7. Asisten memilih **Simpan perubahan**.
8. Sistem memvalidasi seluruh baris dan menyimpan batch beserta log dalam satu transaksi.
9. Sistem memperbarui rekap dan menampilkan notifikasi selama 2 detik.

Jika salah satu baris gagal validasi, seluruh perubahan dalam batch ditolak. Pengguna memperoleh pesan yang menjelaskan baris dan penyebab kegagalannya.

### 5.3 Pemantauan dan tindak lanjut

1. Koordinator memeriksa dashboard dan daftar praktikan berisiko.
2. Koordinator membuka detail praktikan untuk memeriksa modul, nilai, pengumpulan, dan alasan risiko.
3. Koordinator atau asisten membuka tautan tugas untuk pemeriksaan lebih lanjut.
4. Pengguna mengekspor rekap CSV sesuai ruang akses dan filter yang aktif.
5. Koordinator memeriksa log apabila perlu menelusuri perubahan nilai.

Tindak lanjut kepada praktikan dilakukan di luar aplikasi. SIMPRAK tidak mengirim pesan kepada praktikan.

## 6. Kebutuhan fungsional

Penomoran mempertahankan kelompok FR yang disebut dalam komentar kode. Rincian tiap ID ditetapkan dalam dokumen ini. FR-40 dan FR-41 mencakup kebutuhan notifikasi dan keterbacaan terbaru.

### 6.1 Autentikasi dan sesi

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-01 | Pengguna masuk dengan username dan password | Akun aktif dengan kredensial benar memperoleh sesi. Password diverifikasi menggunakan hash BCrypt. |
| FR-02 | Sistem menolak autentikasi yang tidak valid | Input kosong, password salah, akun tidak ditemukan, dan akun nonaktif tidak memperoleh sesi. Pesan gagal tidak mengungkap keberadaan akun. |
| FR-03 | Sistem membatasi akses berdasarkan sesi dan peran | Pengguna tanpa sesi ditolak. Asisten dibatasi pada kelompok bimbingannya, termasuk ketika penugasan kosong. |
| FR-04 | Pengguna dapat keluar dari aplikasi | Logout menghapus sesi. Perubahan nilai yang belum disimpan memerlukan konfirmasi sebelum ditinggalkan. |
| FR-05 | Pengguna dapat mengingat username | Preferensi menyimpan username jika dipilih. Aplikasi tidak menyimpan password untuk login otomatis. |

### 6.2 Dashboard dan rekap

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-06 | Dashboard menampilkan ringkasan praktikum | Jumlah praktikan, kelompok, modul berjalan, rata-rata ketuntasan, dan nilai bersumber dari data sesuai ruang akses. |
| FR-07 | Dashboard menampilkan grafik ketuntasan dan distribusi nilai | Admin melihat ringkasan kelompok. Asisten melihat ringkasan modul kelompoknya. Label grafik terbaca dan indikator ambang sesuai konfigurasi yang digunakan. |
| FR-08 | Pengguna dapat mencari praktikan dari header | Pencarian NRP atau nama membuka daftar praktikan dan menerapkan kata pencarian. Ctrl+K memindahkan fokus ke pencarian. |
| FR-09 | Pengguna dapat menuju daftar praktikan berisiko | Dashboard menampilkan jumlah dan ringkasan praktikan berisiko yang sesuai dengan hak akses. |
| FR-10 | Pengguna dapat mengekspor rekap CSV | CSV memuat data sesuai filter dan hak akses, menggunakan pemisah regional, serta menangani tanda kutip dan baris baru. Filter tanpa hasil tidak mengekspor seluruh data secara diam-diam. |

### 6.3 Praktikan

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-11 | Pengguna dapat melihat daftar praktikan | Daftar menampilkan NRP, nama, kelompok, mata kuliah, angkatan, ketuntasan, nilai akhir, dan status risiko. |
| FR-12 | Pengguna dapat menambahkan praktikan | NRP unik tepat 10 digit, nama, angkatan, mata kuliah, dan kelompok tervalidasi sebelum disimpan. |
| FR-13 | Pengguna dapat mengubah praktikan | Perubahan mengikuti validasi yang sama. Asisten tidak dapat mengambil atau memindahkan data milik kelompok lain. |
| FR-14 | Pengguna dapat menghapus praktikan yang memenuhi syarat | Penghapusan memerlukan konfirmasi dan ditolak jika praktikan memiliki riwayat pengumpulan. |
| FR-15 | Pengguna dapat memfilter daftar praktikan | Filter kelompok, mata kuliah, angkatan, status risiko, dan pencarian dapat digunakan bersama. Hasil kosong menampilkan arahan untuk mengubah filter. |
| FR-16 | Pengguna dapat melihat detail praktikan | Detail menampilkan profil, pengumpulan per modul, nilai, catatan, tautan, dan grafik nilai. Akses mengikuti kelompok bimbingan. |

### 6.4 Kelompok

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-17 | Pengguna dapat melihat kelompok dan ringkasannya | Daftar menampilkan kelompok, asisten, jumlah anggota, dan rekap praktikum sesuai hak akses. |
| FR-18 | Admin dapat menambahkan kelompok | Nomor dan nama kelompok harus unik serta memenuhi validasi. |
| FR-19 | Admin dapat mengubah kelompok dan asisten pembimbing | Pembimbing harus merupakan Asisten aktif yang belum membimbing kelompok lain. Kelompok boleh tanpa pembimbing. |
| FR-20 | Admin dapat menghapus kelompok kosong | Kelompok yang masih memiliki praktikan tidak dapat dihapus. |

### 6.5 Modul

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-21 | Pengguna dapat melihat modul | Daftar memuat kode, judul, kategori, mulai, tenggat, bobot, progres, dan status modul. |
| FR-22 | Admin dapat menambahkan modul | Kode unik, judul, kategori, tanggal, dan bobot tervalidasi. Tenggat tidak boleh sebelum tanggal mulai. |
| FR-23 | Admin dapat mengubah modul | Perubahan tanggal atau bobot tercermin pada status dan rekap setelah pemuatan ulang. |
| FR-24 | Admin dapat menghapus modul | Konfirmasi menjelaskan bahwa pengumpulan dan nilai terkait ikut terhapus. Tindakan tercatat dalam log. |
| FR-25 | Pengguna dapat memfilter status modul | Tab Semua, Berjalan, Selesai, dan Belum Dimulai menampilkan data sesuai waktu acuan. |

### 6.6 Pengumpulan dan penilaian

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-26 | Pengguna dapat memilih modul dan kelompok penilaian | Tabel memuat satu baris per praktikan pada kelompok terpilih. Asisten tidak dapat menilai kelompok lain. |
| FR-27 | Pengguna dapat mencatat waktu pengumpulan | Sistem menerima format waktu yang didukung dan menormalkannya sebelum disimpan. Waktu boleh kosong untuk tugas yang belum dikumpulkan. |
| FR-28 | Sistem menentukan status pengumpulan | Status mengikuti BR-01 dan diperbarui ketika waktu pengumpulan diubah. |
| FR-29 | Pengguna dapat memberikan nilai | Nilai berada pada rentang 0 sampai 100. Nilai ditolak jika belum ada waktu pengumpulan. Nilai kosong berarti belum dinilai. |
| FR-30 | Pengguna dapat menambahkan catatan evaluasi | Catatan opsional tersimpan bersama penilaian dan dapat dibaca pada detail praktikan. |
| FR-31 | Pengguna dapat menandai pengumpulan dengan pintasan | Tombol per baris atau semua baris mengisi waktu saat tindakan dilakukan pada baris yang waktunya masih kosong. |
| FR-32 | Pengguna dapat menyimpan perubahan penilaian secara batch | Baris berubah saja diproses. Semua perubahan data dan log dalam batch harus berhasil bersama atau dibatalkan bersama. |
| FR-33 | Sistem melindungi perubahan yang belum disimpan | Sistem menampilkan jumlah baris berubah. Navigasi, logout, serta pergantian modul atau kelompok tidak boleh menghilangkan perubahan tanpa konfirmasi. |

### 6.7 Risiko, administrasi, dan pengalaman penggunaan

| ID | Kebutuhan | Kriteria penerimaan |
| --- | --- | --- |
| FR-34 | Sistem menampilkan praktikan berisiko | Daftar memuat nilai, ketuntasan, jumlah modul belum dikumpulkan, asisten, dan alasan R1, R2, atau R3. |
| FR-35 | Admin dapat mengubah ambang risiko | Ambang R2 dan R3 dapat diatur dari 10 sampai 100. Nilai awal masing-masing 75 dan 60. Perubahan memperbarui hasil evaluasi. |
| FR-36 | Admin dapat menelusuri log aktivitas | Log menampilkan pelaku, waktu, aksi, entitas, dan keterangan. Perubahan nilai menampilkan nilai lama serta nilai baru. Pencarian dan filter aksi tersedia. |
| FR-37 | Admin dapat mengelola akun pengguna | Admin dapat menambah akun, mengubah data, mereset password, dan mengubah status aktif. Admin tidak dapat menonaktifkan atau mengubah perannya sendiri. |
| FR-38 | Pengguna dapat mengelola pengaturan yang diizinkan | Admin mengubah nama program dan memulihkan data contoh dengan konfirmasi. Pengguna mengubah password sendiri dan memilih tema. |
| FR-39 | Pengguna dapat menyimpan dan membuka tautan tugas | Tautan bersifat opsional. Jika diisi, tautan harus berupa URL HTTP atau HTTPS yang valid. Tautan dapat diganti, dikosongkan, dan dibuka melalui browser. |
| FR-40 | Notifikasi tindakan hilang otomatis | Toast tampil selama 2 detik, dapat ditutup manual, dan waktu tampil dimulai ulang ketika pesan baru menggantikan pesan sebelumnya. |
| FR-41 | Informasi dan tindakan mudah dibaca | Filter memiliki label tetap. Formulir, tabel, grafik, serta tombol terbaca pada kedua tema. Tabel lebar dapat digeser dan teks terpotong memiliki tooltip. |

## 7. Aturan bisnis

### 7.1 BR-01, status pengumpulan dan status modul

| Kondisi | Hasil |
| --- | --- |
| Waktu pengumpulan kosong | Belum Mengumpulkan |
| Waktu pengumpulan lebih awal atau sama dengan tenggat | Tepat Waktu |
| Waktu pengumpulan setelah tenggat | Terlambat |
| Waktu acuan sebelum tanggal mulai modul | Belum Dimulai |
| Waktu acuan sudah mencapai mulai dan belum melewati tenggat | Berjalan |
| Waktu acuan sudah melewati tenggat | Selesai |

Modul dihitung jatuh tempo jika `tenggat < waktu_acuan`. Tepat pada tenggat, modul belum dihitung jatuh tempo. Keterlambatan tidak otomatis mengurangi nilai numerik. Nilai tetap berasal dari penilaian pengguna.

### 7.2 BR-02, integritas dan hak pengelolaan

- NRP unik dan terdiri atas tepat 10 digit angka.
- Angkatan berada pada rentang 2018 sampai 2030 sesuai validasi versi ini.
- Satu praktikan berada pada satu kelompok.
- Satu pasangan praktikan dan modul mempunyai paling banyak satu catatan pengumpulan.
- Bobot modul berupa bilangan bulat dari 1 sampai 100. Jumlah bobot seluruh modul tidak diwajibkan tepat 100.
- Praktikan yang mempunyai riwayat pengumpulan tidak dapat dihapus.
- Kelompok yang masih mempunyai anggota tidak dapat dihapus.
- Penghapusan modul menghapus pengumpulan terkait melalui relasi database, sehingga membutuhkan konfirmasi dampak.
- Admin tidak dapat menonaktifkan akunnya sendiri atau mengubah perannya sendiri.
- Asisten yang dinonaktifkan atau diubah menjadi Admin dilepas dari penugasan pembimbing kelompok.

### 7.3 BR-03, ketuntasan

```text
Ketuntasan = jumlah modul jatuh tempo yang dikumpulkan
             / jumlah seluruh modul jatuh tempo × 100%
```

Pengumpulan terlambat tetap dihitung sebagai pengumpulan. Jika belum ada modul jatuh tempo, ketuntasan bernilai 100%. Hasil dibulatkan menjadi dua angka desimal dengan pembulatan titik tengah menjauhi nol.

### 7.4 BR-04, nilai akhir berbobot

```text
Nilai akhir = jumlah nilai × bobot untuk modul yang sudah dinilai
              / jumlah bobot modul yang masuk perhitungan
```

| Kondisi modul | Perlakuan |
| --- | --- |
| Sudah dinilai, termasuk sebelum tenggat | Nilai dikalikan bobot dan bobot masuk pembagi |
| Sudah jatuh tempo tetapi belum dikumpulkan | Bernilai nol dan bobot masuk pembagi |
| Sudah dikumpulkan tetapi belum dinilai | Tidak masuk pembilang maupun pembagi |
| Belum jatuh tempo dan belum dikumpulkan | Tidak masuk perhitungan |
| Tidak ada bobot yang masuk perhitungan | Nilai akhir nol |

Hasil dibulatkan menjadi dua angka desimal. Contohnya, nilai 90 pada bobot 15 dan satu modul belum dikumpulkan yang sudah jatuh tempo dengan bobot 15 menghasilkan nilai akhir 45. Jika modul kedua sudah dikumpulkan tetapi belum dinilai, nilai akhirnya tetap 90.

### 7.5 BR-05, deteksi praktikan berisiko

| Kriteria | Kondisi | Nilai awal |
| --- | --- | --- |
| R1 | Sedikitnya dua modul jatuh tempo belum dikumpulkan | 2 modul, tetap |
| R2 | Ketuntasan lebih rendah dari ambang ketuntasan | 75% |
| R3 | Nilai akhir lebih rendah dari ambang nilai dan sedikitnya dua modul sudah dinilai | Nilai 60 |

Praktikan berisiko jika memenuhi sedikitnya satu kriteria. Sistem menampilkan semua alasan yang terpenuhi. Ketuntasan tepat 75% atau nilai tepat 60 tidak memicu kriteria terkait pada ambang awal.

## 8. Data dan batas penggunaan

| Entitas | Data utama | Hubungan |
| --- | --- | --- |
| Pengguna | Username, nama, hash password, peran, status aktif, tema, login terakhir | Dapat menjadi pembimbing, penilai, dan pelaku aktivitas |
| Kelompok | Nomor, nama, asisten pembimbing | Memiliki banyak praktikan dan paling banyak satu pembimbing |
| Praktikan | NRP, nama, angkatan, mata kuliah, kelompok | Memiliki pengumpulan untuk setiap modul yang relevan pada versi ini |
| Modul | Kode, judul, kategori, tanggal mulai, tenggat, bobot | Berlaku untuk seluruh praktikan pada database versi ini |
| Pengumpulan | Praktikan, modul, waktu kumpul, tautan, nilai, catatan, penilai, waktu penilaian | Unik untuk setiap pasangan praktikan dan modul |
| Log aktivitas | Pelaku, aksi, entitas, keterangan, waktu | Mencatat tindakan yang dilakukan melalui aplikasi |
| Pengaturan | Nama program, ambang R2, ambang R3, waktu acuan | Menentukan konfigurasi perhitungan dan tampilan |

Waktu tersimpan dalam format `yyyy-MM-dd HH:mm`. Masukan juga mendukung `dd/MM/yyyy HH:mm` dan dinormalkan sebelum disimpan. Versi ini belum melakukan konversi zona waktu.

Database contoh berisi 124 praktikan, 12 kelompok, dan 8 modul. Angka tersebut merupakan data demonstrasi, bukan batas maksimum kapasitas. Pelaporan memasangkan seluruh praktikan dengan seluruh modul. Kolom mata kuliah pada praktikan belum membatasi penerapan modul.

Database lokal berada di direktori data aplikasi pengguna, yaitu `SIMPRAK/simprak.db`. Pada Linux, lokasi umumnya `~/.local/share/SIMPRAK/simprak.db`. Pada Windows, lokasinya `%LOCALAPPDATA%\SIMPRAK\simprak.db`.

## 9. Kebutuhan antarmuka dan aksesibilitas

### 9.1 Layar utama

| Layar | Informasi dan tindakan utama |
| --- | --- |
| Login | Username, password, ingat username, pemilihan tema, dan pengisian akun demo |
| Dashboard | Ringkasan jumlah, ketuntasan, distribusi nilai, praktikan berisiko, dan aktivitas terbaru |
| Daftar praktikan | Pencarian, filter, rekap, detail, tambah, ubah, dan hapus |
| Detail praktikan | Profil, grafik nilai, status setiap modul, catatan, dan tautan tugas |
| Kelompok | Pembimbing, anggota, ringkasan, dan pengelolaan kelompok oleh Admin |
| Modul | Jadwal, bobot, progres, filter status, dan pintasan menuju penilaian |
| Pengumpulan dan nilai | Pemilihan kelompok serta modul, tabel input, penanda perubahan, simpan, dan batal |
| Praktikan berisiko | Ringkasan R1, R2, R3, daftar praktikan, dan alasan risiko |
| Log aktivitas | Pencarian, filter aksi, dan rincian perubahan bagi Admin |
| Pengguna | Pengelolaan akun, peran, status aktif, dan password oleh Admin |
| Pengaturan | Nama program, ambang risiko, tema, perubahan password, dan pemulihan data contoh |

Formulir tambah, ubah, dan konfirmasi ditampilkan melalui dialog pada jendela utama.

### 9.2 Pedoman keterbacaan

- Ukuran minimum jendela 1280 × 740. Ukuran awal mengikuti konfigurasi jendela aplikasi.
- Teks isi umumnya 14 px dan label penting sekurangnya 13 px. Penanda dekoratif boleh lebih kecil.
- Kolom pengisian mempunyai tinggi minimum 42 px. Tombol mempunyai tinggi minimum 40 px.
- Nama praktikan, waktu kumpul, status, dan nilai tetap diprioritaskan sebelum kolom tautan serta catatan pada tabel penilaian.
- Tabel lebar menggunakan pengguliran horizontal. Area tabel yang panjang memiliki pengguliran vertikal dan scrollbar yang terlihat.
- Teks terpotong menyediakan tooltip agar nilai lengkap tetap dapat dibaca.
- Status menggunakan tulisan selain warna. Fokus keyboard harus terlihat.
- Label filter tetap terlihat setelah pilihan diisi. Keadaan tanpa hasil menampilkan tindakan untuk mengubah atau mereset filter.
- Konten dan tombol menggunakan istilah yang dipahami pengguna, seperti **Simpan perubahan**.

### 9.3 Warna utama

| Elemen | Tema terang | Tema gelap |
| --- | --- | --- |
| Aksen utama | `#456D1B` | `#A3D769` |
| Teks tombol utama | `#FFFFFF` | `#14230B` |
| Latar halaman | `#F4F7F2` | `#0D1510` |
| Permukaan | `#FFFFFF` | `#152219` |
| Teks utama | `#111C14` | `#F1F5F0` |
| Teks pendukung | `#435148` | `#C0CEC2` |

Target kontras teks normal sekurangnya 4,5:1. Pengukuran pasangan warna utama tidak menggantikan pemeriksaan seluruh komponen dan keadaan interaksi.

### 9.4 Umpan balik tindakan

Toast menampilkan hasil tindakan tanpa menutup halaman. Toast otomatis hilang setelah 2 detik sejak pesan terakhir ditampilkan. Pengguna tetap dapat menutup toast secara manual.

Toast tidak menggantikan dialog konfirmasi penghapusan atau peringatan perubahan belum disimpan. Galat login tampil pada formulir login. Masukan yang ditolak tidak boleh menyebabkan data yang sedang diedit hilang.

## 10. Kebutuhan nonfungsional

| ID | Kebutuhan | Kriteria penerimaan atau target |
| --- | --- | --- |
| NFR-01 | Operasi lokal | Fitur inti dapat digunakan tanpa internet setelah aplikasi dan dependensi tersedia. |
| NFR-02 | Persistensi | Data yang berhasil disimpan tetap tersedia setelah aplikasi ditutup dan dibuka kembali. |
| NFR-03 | Keamanan akses | Layanan data memeriksa sesi, peran, serta kepemilikan kelompok sebelum operasi yang dibatasi. Password disimpan sebagai hash BCrypt. |
| NFR-04 | Integritas transaksi | Penilaian batch dan log terkait tidak meninggalkan penyimpanan sebagian ketika terjadi kegagalan. |
| NFR-05 | Keterbacaan | Antarmuka memenuhi pedoman pada bagian 9 dalam kedua tema. |
| NFR-06 | Kinerja | Target awal pemuatan ulang dashboard paling lama 2 detik dan penyimpanan satu kelompok contoh paling lama 2 detik pada mesin UAT. Spesifikasi mesin dan hasil pengukuran harus dicatat sebelum target dinyatakan tercapai. |
| NFR-07 | Kompatibilitas | Paket Windows dan Linux dapat menjalankan alur login, pencatatan nilai, penyimpanan ulang, dan ekspor. Keberhasilan pada satu sistem operasi tidak dianggap bukti untuk sistem operasi lain. |
| NFR-08 | Pemeliharaan | Model dan aturan bisnis terpisah dari UI. Perubahan rumus pada C# dan SQL memiliki uji kesesuaian. |
| NFR-09 | Peningkatan database | Database lama yang belum memiliki kolom tautan dapat ditingkatkan tanpa menghapus data yang sudah ada. |
| NFR-10 | Audit | Tindakan aplikasi yang relevan tercatat. Perubahan langsung pada berkas SQLite di luar aplikasi belum tercakup audit. |

NFR-06 merupakan usulan target UAT dan belum menjadi klaim kinerja terukur. Skalabilitas di luar data contoh memerlukan pengujian tersendiri.

## 11. Acuan arsitektur

| Bagian | Tanggung jawab | Teknologi |
| --- | --- | --- |
| `Simprak.App` | Tampilan, navigasi, perintah pengguna, grafik, dan timer toast | Avalonia UI 11.2.3, XAML, CommunityToolkit.Mvvm |
| `Simprak.Core` | Model, DTO, aturan bisnis, sesi, dan pembentukan CSV | C# pada .NET 10 |
| `Simprak.Data` | Inisialisasi SQLite, pemetaan data, validasi, transaksi, dan audit | Entity Framework Core 9, Microsoft.Data.Sqlite, BCrypt |
| `database/simprak.sql` | Skema awal, indeks, trigger, view, dan data demonstrasi | SQLite |
| `Simprak.Tests` | Pengujian aturan, layanan data, dan perilaku toast | xUnit dan Avalonia.Headless |

Saat mulai, aplikasi menyiapkan database, membuat sesi dan layanan data, lalu memasang ViewModel pada jendela utama. UI mengirim tindakan ke layanan data. Layanan menggunakan EF Core untuk pengelolaan data dan query SQLite untuk pelaporan.

Rekap pada UI terutama membaca view SQL. Perhitungan C# menyediakan jalur pembanding bagi pengujian konsistensi. Keduanya perlu diperbarui bersama apabila BR-01 sampai BR-05 berubah.

## 12. Skenario uji penerimaan

| ID | Kondisi dan tindakan | Hasil yang diharapkan | Acuan |
| --- | --- | --- | --- |
| UAT-01 | Akun aktif masuk dengan kredensial benar, lalu salah | Login benar berhasil. Login salah ditolak dengan pesan umum. | FR-01, FR-02 |
| UAT-02 | Asisten mencoba membaca atau mengubah kelompok lain, termasuk tanpa penugasan kelompok | Operasi ditolak atau menampilkan daftar kosong yang menjelaskan belum adanya penugasan. | FR-03, FR-13, FR-26 |
| UAT-03 | Tambahkan NRP duplikat atau bukan 10 digit | Sistem menolak penyimpanan dan mempertahankan masukan untuk diperbaiki. | FR-12, BR-02 |
| UAT-04 | Hapus praktikan yang memiliki pengumpulan atau kelompok yang beranggota | Sistem menolak penghapusan dan menjelaskan alasannya. | FR-14, FR-20 |
| UAT-05 | Waktu kumpul sebelum, tepat pada, dan setelah tenggat | Status berturut-turut Tepat Waktu, Tepat Waktu, dan Terlambat. | FR-28, BR-01 |
| UAT-06 | Tiga dari lima modul jatuh tempo sudah dikumpulkan | Ketuntasan 60%. Jika belum ada modul jatuh tempo, ketuntasan 100%. | BR-03 |
| UAT-07 | Modul bernilai 90 berbobot 15 dan modul jatuh tempo belum dikumpulkan berbobot 15 | Nilai akhir 45. Setelah modul kedua dikumpulkan tanpa nilai, rata-rata menjadi 90. | BR-04 |
| UAT-08 | Praktikan memenuhi R1, R2, serta R3 secara terpisah dan bersamaan | Status dan seluruh alasan yang terpenuhi ditampilkan. | FR-34, BR-05 |
| UAT-09 | Admin mengubah ambang R2 dan R3 | Daftar risiko berubah sesuai ambang. Grafik dan indikator tidak tetap memakai nilai lama yang bertentangan. | FR-07, FR-35 |
| UAT-10 | Satu batch memuat nilai valid dan satu nilai tidak valid | Seluruh batch ditolak dan tidak ada perubahan sebagian maupun log keberhasilan yang menyesatkan. | FR-29, FR-32 |
| UAT-11 | Nilai tersimpan diubah dari 70 menjadi 85 | Nilai menjadi 85 dan log memuat nilai 70, nilai 85, pelaku, serta waktu perubahan. | FR-36 |
| UAT-12 | Pengguna berpindah halaman, logout, mengganti modul, atau mengganti kelompok saat ada perubahan | Sistem meminta konfirmasi. Membatalkan perpindahan mempertahankan masukan. | FR-33 |
| UAT-13 | Simpan URL HTTPS, URL dengan protokol lain, dan tautan kosong | HTTPS valid diterima, protokol lain ditolak, dan tautan kosong diterima. | FR-39 |
| UAT-14 | Filter memiliki hasil, lalu tidak memiliki hasil, kemudian ekspor CSV | Isi ekspor sesuai filter. Hasil kosong tidak berubah menjadi ekspor seluruh praktikan. | FR-10, FR-15 |
| UAT-15 | Tampilkan toast, ganti pesan sebelum habis, lalu tutup manual | Toast hilang setelah 2 detik. Pesan pengganti mendapat 2 detik penuh dan penutupan manual tetap bekerja. | FR-40 |
| UAT-16 | Buka formulir, tabel, dan grafik pada 1280 × 740 dalam kedua tema | Label serta tindakan terbaca, kolom dapat dijangkau melalui scrollbar, dan tooltip menampilkan teks lengkap. | FR-41, NFR-05 |
| UAT-17 | Tutup aplikasi setelah menyimpan penilaian dan buka kembali | Nilai tetap ada dan sesi login perlu dibentuk kembali. | FR-04, FR-05, NFR-02 |
| UAT-18 | Jalankan versi aplikasi pada database lama tanpa kolom tautan | Kolom ditambahkan dan data lama tetap tersedia. | NFR-09 |
| UAT-19 | Admin menghapus modul yang memiliki pengumpulan | Konfirmasi menyebut penghapusan pengumpulan dan nilai terkait sebelum tindakan dijalankan. | FR-24, BR-02 |

Verifikasi terakhir pada sesi pengembangan tanggal 22 Oktober 2026 menghasilkan 47 kasus uji otomatis lulus. Pengujian mencakup aturan bisnis, konsistensi C# dan SQL pada data contoh, beberapa pembatasan akses, audit nilai, tautan, peningkatan skema, CSV, dan timer toast.

Kelulusan tes tersebut tidak berarti seluruh skenario UAT sudah terpenuhi. Bagian 13 mencatat perilaku yang masih perlu diperbaiki atau diputuskan.

## 13. Kesenjangan implementasi dan keputusan terbuka

| ID | Temuan pada implementasi | Kebutuhan atau keputusan tindak lanjut |
| --- | --- | --- |
| GAP-01 | Status modul dan jatuh tempo memakai `waktu_acuan` yang diisi `2026-10-21 23:59` pada data awal. Waktu login, pengumpulan melalui pintasan, dan audit memakai waktu komputer. | Tentukan pemisahan mode demonstrasi dan operasional. Mode operasional memerlukan sumber waktu yang bergerak. Hindari klaim bahwa tenggat selalu mengikuti waktu nyata pada versi saat ini. |
| GAP-02 | Sejumlah query rekap tidak memasang filter ketika ID kelompok asisten kosong. Sebagian pembacaan administratif belum memeriksa peran di layanan. | Terapkan pemeriksaan sesi dan peran pada layanan serta tolak akses asisten tanpa kelompok. Lengkapi UAT-02. |
| GAP-03 | Perubahan praktikan memeriksa kelompok tujuan, tetapi belum memeriksa kepemilikan kelompok asal secara lengkap. | Periksa akses terhadap data yang sudah ada sebelum melakukan perubahan atau pemindahan. |
| GAP-04 | Pergantian modul atau kelompok pada halaman penilaian langsung memuat ulang baris. | Tambahkan perlindungan perubahan belum disimpan pada kedua pilihan tersebut. Konfirmasi navigasi dan logout sudah tersedia. |
| GAP-05 | Ekspor CSV memakai seluruh daftar yang dimuat apabila hasil filter kosong. | Tampilkan pesan hasil kosong atau ekspor header tanpa data. Jangan mengganti ruang ekspor tanpa pemberitahuan. |
| GAP-06 | Sebagian ringkasan dashboard masih memakai angka contoh tetap. Garis dan indikator peringatan grafik masih menggunakan 75 meskipun ambang risiko dapat diubah. | Ganti angka tetap dengan data aktual dan samakan arti indikator dengan konfigurasi. |
| GAP-07 | Layanan `HapusModul` tersedia, tetapi antarmuka belum menyediakan tombol dan perintah penghapusan modul. Relasi database menghapus pengumpulan terkait ketika modul dihapus. | Jika FR-24 dipertahankan, tambahkan tindakan Admin beserta konfirmasi yang menyebut penghapusan pengumpulan dan nilai terkait. |
| GAP-08 | Seluruh modul berlaku bagi seluruh praktikan dan belum ada pemisahan semester. | Pertahankan sebagai batas versi ini. Jika pemisahan dibutuhkan, revisi model data dan BR-03 serta BR-04 sebelum implementasi. |
| GAP-09 | Kesesuaian rumus sudah diuji pada data contoh. Kinerja lintas perangkat dan paket Windows belum dibuktikan oleh pengujian yang tercatat dalam sesi ini. | Lakukan UAT dengan data lebih besar serta paket kedua sistem operasi dan catat hasilnya. |

Bagian ini adalah daftar tindak lanjut. Penyusunan PRD tidak mengubah perilaku aplikasi atau menyatakan bahwa kesenjangan tersebut sudah diperbaiki.

## 14. Prioritas dan syarat selesai

| Tahap | Pekerjaan | Syarat selesai |
| --- | --- | --- |
| P0, integritas dan akses | Tutup GAP-02, GAP-03, GAP-04, GAP-05, dan GAP-07 | Data lintas kelompok tidak terbuka, perubahan tidak hilang tanpa persetujuan, dan tindakan penghapusan serta ekspor tidak mengejutkan pengguna. |
| P0, konsistensi laporan | Putuskan GAP-01 dan perbaiki GAP-06 | Waktu acuan dipahami pengguna. Ringkasan dan ambang sesuai data serta konfigurasi. |
| P1, penerimaan operasional | Jalankan UAT, uji paket Windows dan Linux, serta ukur target kinerja | Hasil pengujian dicatat, kegagalan ditangani, dan koordinator meninjau hasilnya. |
| Pengembangan berikutnya | Evaluasi kebutuhan semester, modul per mata kuliah, cadangan, serta impor | Kebutuhan disetujui melalui perubahan ruang lingkup sebelum pengerjaan. |

Produk dinyatakan memenuhi PRD setelah kebutuhan wajib dan UAT terkait lulus, tanpa kesenjangan P0 yang terbuka. Tanggal rilis, penanggung jawab penerimaan, dan spesifikasi mesin UAT belum ditetapkan dalam dokumen ini.

## 15. Referensi proyek

- [README proyek](README.md).
- [Model data](src/Simprak.Core/Models/Entities.cs).
- [Aturan bisnis](src/Simprak.Core/Rules/SimprakRules.cs).
- [Sesi dan pembentukan CSV](src/Simprak.Core/Services/SessionAndCsv.cs).
- [Layanan data dan transaksi penilaian](src/Simprak.Data/Services/SimprakDataService.cs).
- [Inisialisasi database](src/Simprak.Data/DatabaseInitializer.cs).
- [Skema, view, dan data awal](database/simprak.sql).
- [ViewModel utama dan timer notifikasi](src/Simprak.App/ViewModels/MainWindowViewModel.cs).
- [Tampilan aplikasi](src/Simprak.App/Views/MainWindow.axaml).
- [Palet tema](src/Simprak.App/Services/AppServices.cs).
- [Pengujian aturan dan layanan data](tests/Simprak.Tests/SimprakComprehensiveTests.cs).
- [Pengujian notifikasi](tests/Simprak.Tests/ToastTests.cs).
