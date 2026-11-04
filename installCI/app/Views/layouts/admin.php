<!DOCTYPE html>
<html lang="id">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Dashboard Admin</title>
    <link href="<?= base_url('sb-admin-2/vendor/fontawesome-free/css/all.min.css') ?>" rel="stylesheet">
    <link href="<?= base_url('sb-admin-2/css/sb-admin-2.min.css') ?>" rel="stylesheet">
    <link href="<?= base_url('sb-admin-2/css/admin-custom.css') ?>" rel="stylesheet">
</head>
<body id="page-top">
<nav class="admin-topbar navbar navbar-dark bg-dark">
    <a class="navbar-brand" href="<?= base_url('dashboard') ?>">Admin PBKK</a>
    <button class="btn text-white-50" id="sidebarToggleTop" type="button" aria-label="Ubah tampilan sidebar" aria-controls="accordionSidebar"><i class="fas fa-bars"></i></button>
    <span class="ml-auto text-white-50 small">Dashboard Admin</span>
</nav>
<div id="wrapper">
    <ul class="navbar-nav sidebar sidebar-dark" id="accordionSidebar">
        <li class="nav-item active">
            <a class="nav-link" href="<?= base_url('dashboard') ?>"><i class="fas fa-fw fa-tachometer-alt"></i> <span>Dashboard</span></a>
        </li>
        <li class="nav-item">
            <a class="nav-link" href="<?= base_url('profile') ?>"><i class="fas fa-fw fa-user"></i> <span>Profile</span></a>
        </li>
        <li class="nav-item">
            <a class="nav-link" href="<?= base_url('about') ?>"><i class="fas fa-fw fa-info-circle"></i> <span>About</span></a>
        </li>
    </ul>
    <div id="content-wrapper" class="d-flex flex-column">
        <main id="content" class="container-fluid py-3">
            <?= $this->renderSection('content') ?>
        </main>
        <footer class="admin-footer mt-auto py-4 text-center small">Admin PBKK &copy; <?= date('Y') ?></footer>
    </div>
</div>
<script src="<?= base_url('sb-admin-2/vendor/jquery/jquery.min.js') ?>"></script>
<script src="<?= base_url('sb-admin-2/vendor/bootstrap/js/bootstrap.bundle.min.js') ?>"></script>
<script src="<?= base_url('sb-admin-2/vendor/jquery-easing/jquery.easing.min.js') ?>"></script>
<script src="<?= base_url('sb-admin-2/js/sb-admin-2.min.js') ?>"></script>
<?= $this->renderSection('scripts') ?>
</body>
</html>
