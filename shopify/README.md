# Shopify — katalog dan belanja

Aplikasi toko CodeIgniter 4. Dari folder `shopify`, jalankan:

```bash
composer install
php spark serve
```

Buka http://localhost:8080. PHP memerlukan ekstensi PDO SQLite dan fileinfo. Aplikasi membuat `writable/shop.db` serta produk contoh secara otomatis. Pastikan `writable` dan `public/uploads/products` dapat ditulis server.

Pembeli dapat mendaftar di menu Akun untuk melihat riwayat, atau checkout tanpa akun dan menyimpan tautan invoice. Stok berkurang ketika pesanan dibuat. Perubahan status oleh admin tidak mengembalikan stok otomatis. Omzet adalah total pesanan Completed.

Admin dapat masuk melalui `/login`, lalu membuka `/admin/shop`. Akun demo dikonfigurasi pada `app/Config/DemoAdmin.php`; ganti sebelum aplikasi diakses publik.

## MySQL opsional

Impor `shop.sql` ke database kosong dan aktifkan PDO MySQL. Atur environment variables `SHOP_DSN` (misalnya `mysql:host=127.0.0.1;dbname=shop;charset=utf8mb4`), `SHOP_DB_USER`, dan `SHOP_DB_PASSWORD` untuk proses PHP. Produk contoh dibuat jika tabel produk masih kosong.

## Tes

Jalankan `vendor/bin/phpunit tests/unit/ShopStoreTest.php` dari folder `shopify`.
