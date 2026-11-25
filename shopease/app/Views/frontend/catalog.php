<?= $this->extend('layouts/store') ?>
<?= $this->section('content') ?>

<div class="container">
    <header class="page-intro">
        <h1>Katalog</h1>
        <p>Jelajahi pilihan produk ShopEase berdasarkan kategori. Harga dan produk yang ditampilkan adalah data demo.</p>
        <?php if ($categories !== []): ?>
            <nav class="category-jumps" aria-label="Kategori katalog">
                <?php foreach ($categories as $slug => $category): ?>
                    <a href="#<?= esc($slug, 'attr') ?>"><?= esc($category['name']) ?></a>
                <?php endforeach ?>
            </nav>
        <?php endif ?>
    </header>

    <?php if ($categories === []): ?>
        <p class="empty-state">Belum ada kategori di katalog.</p>
    <?php else: ?>
        <?php foreach ($categories as $slug => $category): ?>
            <section class="store-section catalog-section" id="<?= esc($slug, 'attr') ?>" tabindex="-1"
                     aria-labelledby="heading-<?= esc($slug, 'attr') ?>">
                <h2 id="heading-<?= esc($slug, 'attr') ?>"><?= esc($category['name']) ?></h2>
                <?= view('components/product_grid', [
                    'products' => $productsByCategory[$slug] ?? [],
                    'categories' => $categories,
                    'emptyMessage' => 'Belum ada produk dalam kategori ini.',
                ]) ?>
            </section>
        <?php endforeach ?>
    <?php endif ?>
</div>

<?= $this->endSection() ?>
