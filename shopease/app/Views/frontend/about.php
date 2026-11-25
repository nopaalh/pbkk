<?= $this->extend('layouts/store') ?>
<?= $this->section('content') ?>

<div class="container">
    <div class="info-page row g-0">
        <div class="col-md-7 info-copy">
            <h1>Tentang ShopEase</h1>
            <p class="info-lead">Katalog pilihan untuk menemani rutinitas dan gaya sehari-hari.</p>
            <p>ShopEase mengelompokkan produk elektronik, fashion, dan aksesori agar lebih mudah dijelajahi. Pilih kategori yang kamu cari atau mulai dari produk terbaru di beranda.</p>
            <p>ShopEase dibuat sebagai proyek praktikum PBKK untuk mendemonstrasikan routing, controller, dan view di CodeIgniter 4.</p>
            <a class="btn store-button" href="<?= esc(url_to('catalog'), 'attr') ?>">Lihat katalog <span aria-hidden="true">↗</span></a>
        </div>
        <div class="col-md-5 info-image">
            <?= view('components/photo', [
                'image' => 'collection-studio.png',
                'frame' => 'top-left',
                'description' => 'Koleksi pilihan ShopEase',
            ]) ?>
        </div>
    </div>
</div>

<?= $this->endSection() ?>
