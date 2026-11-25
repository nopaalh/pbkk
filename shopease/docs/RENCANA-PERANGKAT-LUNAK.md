# Rencana Perangkat Lunak ShopEase

**Status:** Draf v0.1 untuk persetujuan  
**Lokasi proyek:** `tugas/shopease/`

## Tujuan dan acuan tugas

ShopEase adalah toko online untuk beberapa kategori produk. Berandanya membantu pengunjung mengenali toko, menemukan kategori, dan mulai menjelajahi katalog. Untuk praktikum PBKK ini, beranda juga menjadi contoh cara CodeIgniter 4 menerima URL, menjalankan controller, dan merender view.

Bagian **Tugas** meminta landing page ShopEase yang mengacu pada PRD serta modifikasi routing, controller, dan view. Hands-On Lab (HOL) Praktikum 3 juga meminta halaman About dan Contact sebagai latihan navigasi. PRD dan studi kasus menggambarkan produk lengkap yang menjadi acuan pengembangan berikutnya. [Halaman tugas Pak Fajar](https://fajarbaskoro.blogspot.com/2021/05/mvc-dan-routing-ci.html) · [Hands-On Lab, Praktikum 3](https://docs.google.com/document/d/1UeP6gfr6OMpOmESK3LJFENQwyrL5HnUQ/edit)

## Fitur untuk praktikum ini

| Fitur | Perilaku | Kriteria penerimaan |
| --- | --- | --- |
| Beranda dan navigasi | Header ShopEase, banner promosi, dan tombol **Lihat katalog**. Menu mengarah ke Beranda, Katalog, Tentang, dan Kontak. | `GET /` memuat beranda. Semua tautan menuju halaman yang sesuai. |
| Kategori | Pengunjung memilih Elektronik, Fashion, atau Aksesori dari beranda. | Tautan kategori membuka bagian yang sesuai di katalog. |
| Produk terbaru dan terlaris | Beranda menampilkan nama, gambar, dan harga contoh untuk setiap kelompok produk. | Urutan terbaru mengikuti tanggal penambahan. Urutan terlaris mengikuti angka penjualan contoh. Keduanya berasal dari kumpulan data yang sama. |
| Katalog ringkas | Halaman baca-saja menampilkan produk contoh per kategori. | Katalog dapat dibuka langsung dari URL, tombol di banner, dan menu. Kartu informasi konsisten dengan beranda. |
| Tentang dan Kontak | Halaman Tentang menjelaskan tujuan toko. Halaman Kontak menampilkan informasi kontak demo. | Kedua halaman dapat dibuka lewat menu dan URL langsung. Kontak demo diberi label yang jelas. |
| Tampilan responsif | Pengunjung bisa membaca dan memakai menu serta tombol di desktop dan ponsel. | Pada lebar 375 px dan 1.280 px, teks terbaca, tautan dapat digunakan lewat keyboard, dan halaman tidak meluber secara horizontal. |

Banner, kategori, produk terbaru, produk terlaris, dan tautan katalog mengikuti deskripsi homepage dalam studi kasus. Katalog ringkas dengan data contoh, kategori yang dipilih, dan target lebar layar adalah usulan untuk demo. [Deskripsi studi kasus, bagian 4.1](https://docs.google.com/document/d/1abCtPjlZ6X9Qz5QLAPnL_GETj9Z8dfAF/edit)

## Pendekatan dan rancangan teknis

Gunakan proyek CodeIgniter 4 terpisah di `shopease/` dengan PHP, Composer, tampilan PHP, dan Bootstrap 5. Satu layout menampung navigasi dan footer supaya keempat halaman memiliki struktur yang sama. Katalog berisi data contoh dalam satu berkas konfigurasi sehingga demo routing dapat dijalankan tanpa menyiapkan database. MySQL dan Model CodeIgniter masuk saat katalog mulai dikelola dan datanya perlu disimpan.

| Metode dan URL | Controller | View |
| --- | --- | --- |
| `GET /` | `HomeController::index` | `frontend/home` |
| `GET /katalog` | `CatalogController::index` | `frontend/catalog` |
| `GET /about` | `HomeController::about` | `frontend/about` |
| `GET /contact` | `HomeController::contact` | `frontend/contact` |

1. `app/Config/Routes.php` memetakan URL publik ke metode controller secara eksplisit.
2. Controller menyiapkan judul dan data tampilan. Controller beranda memilih produk terbaru berdasarkan tanggal dan produk terlaris berdasarkan jumlah terjual dari data contoh. Jika nilainya sama, urutkan menurut nama.
3. View merender layout, kartu produk, dan tautan. Gunakan `esc()` untuk data yang ditampilkan dan helper URL CodeIgniter untuk tautan internal.

Untuk produk tanpa foto, tampilkan gambar pengganti lokal. Jika kelompok produk atau kategori kosong, tampilkan pesan singkat dan pertahankan navigasi. Tampilkan halaman 404 standar untuk URL yang tidak terdaftar.

## Roadmap produk lengkap

| Fase setelah landing page | Fitur | Hasil yang diperiksa |
| --- | --- | --- |
| Katalog dan administrasi | MySQL, tabel kategori dan produk, CRUD admin, foto, harga, stok, pencarian, filter, pengurutan, pagination, dan detail produk. | Perubahan admin tercermin di katalog. Produk nonaktif tersembunyi, dan gambar yang gagal validasi ditolak. |
| Akun dan transaksi | Registrasi, login, logout, peran pelanggan/admin/operator, keranjang, alamat, checkout, dan riwayat. | Server menghitung ulang total dan menyimpan harga saat transaksi. Pesanan, rincian, dan stok konsisten ketika transaksi gagal atau stok tidak cukup. Pelanggan hanya bisa melihat pesanannya sendiri. |
| Pembayaran dan pengiriman | Bukti transfer, verifikasi admin, pemrosesan operator, kurir, nomor resi, dan pelacakan status. | Bukti yang ditolak bisa dikirim ulang. Operator memproses pesanan yang sudah lolos verifikasi dan pelanggan bisa melihat perubahan status. |
| Dashboard dan laporan | Kelola data pelanggan, ringkasan pesanan dan stok, laporan penjualan, dan dokumentasi pengujian. | Ringkasan sesuai transaksi dan laporan sesuai periode yang dipilih. |

Integrasi payment gateway, ekspedisi otomatis, dan marketplace multi-vendor berada di luar versi awal PRD. [PRD ShopEase, ruang lingkup dan milestone](https://docs.google.com/document/d/19P2k4LBXTK8nrcvd5j1mvFt-fHhlSdyx/edit)

## Urutan kerja dan pembagian peran

| Urutan | Penanggung jawab yang diusulkan | Dependensi dan hasil |
| --- | --- | --- |
| 1. Setujui cakupan | Pengguna dan PM Lead | Tetapkan fitur praktikum, data demo, dan target presentasi. |
| 2. Siapkan proyek dan rancangan route | Farhan, dengan tinjauan struktur dari Risky | Setelah persetujuan. Jalankan proyek CI4 baru, hubungkan empat route, dan cek bahwa setiap URL menjangkau controller yang tepat. |
| 3. Isi data dan buat tampilan | Farhan | Setelah kerangka route siap. Buat layout, beranda, katalog, Tentang, dan Kontak. |
| 4. Review dan perbaiki | Agung untuk review, Farhan untuk perbaikan | Setelah halaman siap. Periksa tautan, responsivitas, kasus kosong, dan akses URL yang tidak terdaftar. |
| 5. Siapkan demo dan laporan | PM Lead | Setelah perbaikan diperiksa ulang. Simpan tangkapan layar desktop dan ponsel, daftar URL dan controller, serta hasil uji. |

## Pemeriksaan dan alur demo

- Jalankan `php spark routes` dan cocokkan empat URL dengan controller. Jalankan `php spark serve`, lalu cek bahwa empat URL berstatus 200 dan URL yang tidak terdaftar berstatus 404.
- Dari beranda, klik **Lihat katalog**, buka satu kategori, lalu kunjungi Tentang dan Kontak. Cocokkan nama dan harga produk antara beranda dan katalog.
- Cek tampilan desktop dan ponsel, navigasi keyboard, data kosong, serta kondisi ketika gambar produk tidak tersedia.
- Saat demo, buka beranda, jelajahi katalog, lalu tunjukkan satu contoh perjalanan URL → controller → view agar penerapan materi terlihat.

Data contoh memungkinkan demo tanpa database sambil menyiapkan jalur migrasi ke MySQL. Gunakan aset gambar lokal yang boleh dipakai agar demo tidak bergantung pada tautan luar. Utamakan fitur yang ada di bagian **Tugas** dan HOL Praktikum 3 bila waktu pengerjaan terbatas.
