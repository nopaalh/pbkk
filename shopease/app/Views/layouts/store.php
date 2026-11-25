<?php
$navigation = ['home' => 'Beranda', 'catalog' => 'Katalog', 'about' => 'Tentang', 'contact' => 'Kontak'];
?>
<!doctype html>
<html lang="id">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title><?= esc($title) ?> | ShopEase</title>
    <link rel="icon" href="<?= esc(base_url('assets/images/mark.svg'), 'attr') ?>" type="image/svg+xml">
    <link rel="stylesheet" href="<?= esc(base_url('assets/bootstrap/bootstrap.min.css'), 'attr') ?>">
    <link rel="stylesheet" href="<?= esc(base_url('assets/shop.css'), 'attr') ?>">
    <script defer src="<?= esc(base_url('assets/bootstrap/bootstrap.bundle.min.js'), 'attr') ?>"></script>
    <script defer src="<?= esc(base_url('assets/images.js'), 'attr') ?>"></script>
</head>
<body>
<a class="skip-link" href="#main">Lewati ke konten</a>
<header class="site-header">
    <nav class="navbar navbar-expand-md" aria-label="Navigasi utama">
        <div class="container">
            <a class="navbar-brand brand" href="<?= esc(url_to('home'), 'attr') ?>">
                <img src="<?= esc(base_url('assets/images/mark.svg'), 'attr') ?>" width="37" height="37" alt="">
                ShopEase
            </a>
            <button class="navbar-toggler menu-toggle" type="button"
                    data-bs-toggle="collapse" data-bs-target="#site-navigation"
                    aria-controls="site-navigation" aria-expanded="false" aria-label="Buka atau tutup menu">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="site-navigation">
                <ul class="navbar-nav ms-auto">
                    <?php foreach ($navigation as $route => $label): ?>
                        <li class="nav-item">
                            <a class="nav-link<?= $page === $route ? ' active' : '' ?>"
                               href="<?= esc(url_to($route), 'attr') ?>"<?= $page === $route ? ' aria-current="page"' : '' ?>>
                                <?= esc($label) ?>
                            </a>
                        </li>
                    <?php endforeach ?>
                </ul>
            </div>
        </div>
    </nav>
</header>

<main id="main" tabindex="-1">
    <?= $this->renderSection('content') ?>
</main>

<footer class="site-footer">
    <div class="container d-flex flex-wrap align-items-center justify-content-between gap-3">
        <span class="footer-brand">ShopEase</span>
        <span>Katalog demo · Praktikum PBKK</span>
    </div>
</footer>
</body>
</html>
