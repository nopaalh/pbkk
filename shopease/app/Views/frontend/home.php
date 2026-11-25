<?= $this->extend('layouts/store') ?>
<?= $this->section('content') ?>

<div class="container">
    <section class="hero" aria-labelledby="hero-title">
        <div class="hero-copy">
            <span class="hero-kicker">Pilihan sehari-hari</span>
            <h1 id="hero-title">Temukan barang yang pas untukmu.</h1>
            <p>Dari perangkat audio sampai aksesori, jelajahi pilihan ShopEase untuk rutinitas dan gaya sehari-hari.</p>
            <a class="btn store-button" href="<?= esc(url_to('catalog'), 'attr') ?>">
                Lihat katalog <span aria-hidden="true">↗</span>
            </a>
        </div>
        <div class="hero-visual">
            <?= view('components/photo', [
                'image' => 'collection-studio.png',
                'frame' => 'top-left',
                'description' => 'Headphone, tas kuning, dan jam tangan di atas panggung hijau',
                'eager' => true,
            ]) ?>
        </div>
    </section>

    <section class="store-section" aria-labelledby="categories-title">
        <h2 id="categories-title">Belanja per kategori</h2>
        <?php if ($categories === []): ?>
            <p class="empty-state">Belum ada kategori yang tersedia.</p>
        <?php else: ?>
            <div class="row row-cols-1 row-cols-md-3 g-3">
                <?php foreach ($categories as $slug => $category): ?>
                    <div class="col">
                        <a class="category-tile category-tile--<?= esc($slug, 'attr') ?>"
                           href="<?= esc(url_to('catalog') . '#' . $slug, 'attr') ?>">
                            <span class="category-icon"><?= view('components/category_icon', ['category' => $slug]) ?></span>
                            <span><strong><?= esc($category['name']) ?></strong><small><?= esc($category['description']) ?></small></span>
                            <span class="category-arrow" aria-hidden="true">↗</span>
                        </a>
                    </div>
                <?php endforeach ?>
            </div>
        <?php endif ?>
    </section>

    <section class="store-section" aria-labelledby="latest-title">
        <div class="section-heading">
            <h2 id="latest-title">Produk terbaru</h2>
            <a class="text-link" href="<?= esc(url_to('catalog'), 'attr') ?>">Lihat semua produk</a>
        </div>
        <?= view('components/product_grid', ['products' => $latest, 'categories' => $categories, 'emptyMessage' => 'Belum ada produk terbaru.']) ?>
    </section>

    <section class="store-section" aria-labelledby="bestsellers-title">
        <h2 id="bestsellers-title">Produk terlaris</h2>
        <?= view('components/product_grid', ['products' => $bestsellers, 'categories' => $categories, 'emptyMessage' => 'Belum ada produk terlaris.']) ?>
    </section>
</div>

<?= $this->endSection() ?>
