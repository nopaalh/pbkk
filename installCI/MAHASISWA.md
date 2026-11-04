# Modul mahasiswa

Konfigurasikan MySQL/MariaDB di `.env` dan jalankan `php spark migrate`. Login melalui `/login` menggunakan tombol akun demo, lalu buka `/mahasiswa` untuk menambah, melihat, mengubah, dan menghapus data. Login demo tidak membutuhkan database, tetapi CRUD mahasiswa membutuhkannya.

Foto JPG, PNG, atau WebP maksimal 2 MB bersifat opsional dan disimpan di `writable/uploads/mahasiswa`, bukan di `public/`. Foto disajikan melalui route yang memerlukan login. Pastikan direktori `writable` dapat ditulis dan tidak dapat diakses langsung dari web. Foto lama dihapus setelah pembaruan atau penghapusan data berhasil.

Jalankan tes dengan `vendor/bin/phpunit`. Tes database menggunakan konfigurasi `tests` pada `app/Config/Database.php` dan membutuhkan ekstensi PHP SQLite3.
