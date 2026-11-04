# Sistem akademik

Aplikasi menggunakan SQLite, sehingga tidak memerlukan server MySQL. Pastikan ekstensi SQLite3 tersedia untuk PHP yang digunakan (`php -m | grep -i sqlite`). Pada Ubuntu dengan PHP 8.5, pasang melalui `sudo apt install php8.5-sqlite3`, lalu jalankan:

```bash
php spark migrate
php spark serve
```

Buka `http://localhost:8080/login`, isi akun demo, lalu login. Dashboard ada di `http://localhost:8080/dashboard` dan daftar mahasiswa di `http://localhost:8080/mahasiswa`. Profil dan tentang dapat dibuka dari sidebar dashboard. File database dibuat di `writable/database/akademik.sqlite`; direktori `writable` harus dapat ditulis. File database tidak disertakan dalam Git.

Foto JPG, PNG, atau WebP maksimal 2 MB bersifat opsional dan disimpan di `writable/uploads/mahasiswa`, bukan di `public/`. Foto disajikan melalui route yang memerlukan login. Foto lama dihapus setelah pembaruan atau penghapusan data berhasil.

Jalankan tes dengan `vendor/bin/phpunit`. Tes database memakai SQLite di memori dan juga membutuhkan ekstensi PHP SQLite3.
