<?php if ($products === []): ?>
    <p class="empty-state"><?= esc($emptyMessage) ?></p>
<?php else: ?>
    <div class="row row-cols-1 row-cols-sm-2 row-cols-lg-3 g-4">
        <?php foreach ($products as $product): ?>
            <div class="col">
                <article class="product-card h-100" data-product="<?= esc($product['id'], 'attr') ?>">
                    <?= view('components/photo', ['image' => $product['image'] ?? null, 'frame' => $product['frame'] ?? 'top-left']) ?>
                    <div class="product-info">
                        <p class="product-category"><?= esc($categories[$product['category']]['name'] ?? 'Produk') ?></p>
                        <h3><?= esc($product['name']) ?></h3>
                        <p class="product-price">Rp<?= number_format($product['price'], 0, ',', '.') ?></p>
                    </div>
                </article>
            </div>
        <?php endforeach ?>
    </div>
<?php endif ?>
