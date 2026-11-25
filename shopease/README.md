# ShopEase

ShopEase adalah demo katalog toko online untuk praktikum PBKK tentang routing, controller, dan view di CodeIgniter 4. Beranda menampilkan banner, kategori Elektronik, Fashion, dan Aksesori, serta tiga produk terbaru dan tiga terlaris. Katalog menampilkan tujuh produk contoh berdasarkan kategori. Halaman Tentang menjelaskan proyek dan halaman Kontak menampilkan informasi demo.

[Acuan tugas Pak Fajar](https://fajarbaskoro.blogspot.com/2021/05/mvc-dan-routing-ci.html) · [Panduan praktikum](https://docs.google.com/document/d/1UeP6gfr6OMpOmESK3LJFENQwyrL5HnUQ/edit) · [Rencana perangkat lunak](docs/RENCANA-PERANGKAT-LUNAK.md) · [Laporan praktikum](docs/LAPORAN.md)

## Jalankan di komputer lokal

Siapkan PHP 8.2 atau lebih baru dengan ekstensi `intl` dan `mbstring`, serta Composer. Untuk memasang dan menjalankan PHPUnit, aktifkan juga ekstensi XML PHP, termasuk `dom` dan `xmlwriter`. Jalankan dari folder ini.

```bash
composer install
php spark serve --host localhost --port 8081
```

Buka [ShopEase](http://localhost:8081/). Port 8081 dipilih karena Shopify berjalan di 8080. Jika memakai port lain, salin `env` ke `.env`, setel `app.baseURL` ke URL dengan port tersebut, lalu jalankan server dengan argumen `--port` yang sama.

Bootstrap 5.3.9, ikon, dan gambar disajikan dari `public/assets/`. Demo bisa dibuka tanpa koneksi internet setelah Composer selesai memasang dependensi.

## Empat route publik

| URL | Controller | View |
| --- | --- | --- |
| `GET /` | `HomeController::index` | `frontend/home` |
| `GET /katalog` | `CatalogController::index` | `frontend/catalog` |
| `GET /about` | `HomeController::about` | `frontend/about` |
| `GET /contact` | `HomeController::contact` | `frontend/contact` |

Pemetaan ada di `app/Config/Routes.php`. Auto routing nonaktif. Contoh alur demo dari `GET /katalog` adalah route → `CatalogController::index` → pengelompokan data dari `app/Config/DemoCatalog.php` → `frontend/catalog` yang memakai layout bersama `app/Views/layouts/store.php`. Beranda dan katalog memakai komponen kartu serta gambar yang sama.

## Verifikasi yang dijalankan

```bash
php spark routes
vendor/bin/phpunit --no-coverage tests/feature
```

- **Server lokal.** Empat URL di atas mengembalikan HTTP 200. URL yang tidak dikenal mengembalikan 404.
- **Data katalog.** Enam tes fitur (total 11 tes PHPUnit dengan 41 asersi) lulus. Tes mencakup route, auto-routing nonaktif, urutan berdasarkan tanggal penambahan dan jumlah penjualan contoh, urutan nama saat hasil seri, kecocokan nama dan harga dengan katalog, kecocokan tautan kategori dengan bagian katalog, data kosong, dan pemilihan gambar pengganti lokal.
- **Browser.** Menu menuju empat halaman pada lebar 1.280 px dan 375 px, tanpa gulir horizontal. Tiga tautan kategori beranda menuju bagian katalog yang tepat. Tombol **Lihat katalog** membuka katalog.
- **Keyboard.** Tombol Tab menampilkan fokus pada tautan lompat ke konten, tombol katalog, dan tombol menu ponsel. Enter membuka tujuan atau memperluas menu. Tab masuk ke tautan setelah menu terbuka.
- **Gambar gagal dimuat.** URL gambar banner yang sengaja dibikin tidak tersedia di tab uji diganti oleh gambar pengganti lokal.

Pengujian browser menggunakan satu browser berbasis Chromium dengan ukuran layar yang disimulasikan. Produk, harga, penjualan, dan kontak adalah data demo. Aplikasi tahap ini berupa katalog baca-saja tanpa basis data dan transaksi.

## Aset

- `collection-studio.png` dan `collection-essentials.png` adalah gambar promosi dan contoh produk asli yang dibuat untuk ShopEase dengan generator gambar dan disimpan lokal. Tiap berkas memuat empat bingkai gambar yang ditampilkan sesuai kartu.
- Logo, ikon kategori, dan gambar pengganti dibuat untuk proyek ini.
- Bootstrap disertakan dengan [lisensi MIT](public/assets/bootstrap/LICENSE).
