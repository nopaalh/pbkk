<?php $path = uri_string(); ?>
<!DOCTYPE html>
<html lang="id">
<head>
    <meta charset="utf-8"><meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Sistem Akademik</title>
    <link href="<?= base_url('sb-admin-2/vendor/fontawesome-free/css/all.min.css') ?>" rel="stylesheet">
    <link href="<?= base_url('sb-admin-2/css/sb-admin-2.min.css') ?>" rel="stylesheet">
    <link href="<?= base_url('sb-admin-2/css/admin-custom.css') ?>" rel="stylesheet">
</head>
<body>
<div class="app-shell">
    <aside class="app-sidebar">
        <a class="brand" href="<?= site_url('dashboard') ?>"><span class="brand-icon"><i class="fas fa-graduation-cap"></i></span> Akademika<span class="brand-dot">.</span></a>
        <div class="workspace"><span class="workspace-icon"><i class="fas fa-university"></i></span><span><strong>Ruang akademik</strong><small>Panel administrator</small></span></div>
        <div class="nav-label">Menu utama</div>
        <nav aria-label="Navigasi utama">
            <a class="side-link <?= $path === 'dashboard' ? 'is-active' : '' ?>" href="<?= site_url('dashboard') ?>"><i class="fas fa-th-large"></i> Dashboard</a>
            <a class="side-link <?= str_starts_with($path, 'mahasiswa') ? 'is-active' : '' ?>" href="<?= site_url('mahasiswa') ?>"><i class="fas fa-user-graduate"></i> Mahasiswa</a>
            <div class="nav-label">Informasi</div>
            <a class="side-link <?= $path === 'dashboard/profile' ? 'is-active' : '' ?>" href="<?= site_url('dashboard/profile') ?>"><i class="fas fa-user-circle"></i> Profile</a>
            <a class="side-link <?= $path === 'dashboard/about' ? 'is-active' : '' ?>" href="<?= site_url('dashboard/about') ?>"><i class="fas fa-info-circle"></i> About</a>
        </nav>
        <div class="sidebar-bottom">PBKK <span>•</span> CodeIgniter 4</div>
    </aside>
    <div class="app-main">
        <header class="app-header"><div><span class="header-caption">Sistem informasi akademik</span><strong>Ruang administrasi</strong></div><div class="header-account"><span class="account-avatar">A</span><span><strong>Administrator</strong><small><?= esc(session()->get('user_email')) ?></small></span><form action="<?= site_url('logout') ?>" method="post"><?= csrf_field() ?><button class="logout-button" type="submit" title="Logout" aria-label="Logout"><i class="fas fa-sign-out-alt"></i></button></form></div></header>
        <main class="app-content"><?= $this->renderSection('content') ?></main>
        <footer class="app-footer">Sistem Akademik <span>© <?= date('Y') ?> PBKK</span></footer>
    </div>
</div>
<?= $this->renderSection('scripts') ?>
</body>
</html>
