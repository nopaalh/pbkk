<!doctype html>
<html lang="id">
<head>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <title>Login | Akademika</title>
    <link rel="stylesheet" href="<?= base_url('sb-admin-2/vendor/fontawesome-free/css/all.min.css') ?>">
    <link rel="stylesheet" href="<?= base_url('sb-admin-2/css/login.css') ?>">
</head>
<body>
<main class="login-shell">
    <section class="login-intro" aria-label="Tentang Akademika">
        <a class="brand" href="<?= site_url('login') ?>"><span class="brand-icon"><i class="fas fa-graduation-cap" aria-hidden="true"></i></span>Akademika<span class="brand-dot">.</span></a>
        <div class="intro-content">
            <span class="intro-label"><span></span> SISTEM INFORMASI AKADEMIK</span>
            <h1>Ruang yang lebih sederhana untuk data akademik.</h1>
            <p>Kelola informasi mahasiswa dan lihat ringkasan data dalam satu tempat.</p>
            <div class="preview-card" aria-hidden="true">
                <div class="preview-heading"><span class="preview-icon"><i class="fas fa-user-graduate"></i></span><div><strong>Data mahasiswa</strong><small>Ringkasan akademik</small></div><i class="fas fa-ellipsis-h preview-more"></i></div>
                <div class="preview-lines"><span></span><span></span></div>
                <div class="preview-footer"><span class="preview-avatars"><i>A</i><i>M</i><i>S</i></span><span>Semua data, satu tempat</span></div>
            </div>
        </div>
        <p class="intro-footer">Akademika &nbsp;·&nbsp; PBKK</p>
    </section>
    <section class="login-content" aria-labelledby="login-title">
        <div class="mobile-brand"><span class="brand-icon"><i class="fas fa-graduation-cap" aria-hidden="true"></i></span>Akademika<span class="brand-dot">.</span></div>
        <div class="login-form-wrap">
            <div class="form-symbol"><i class="fas fa-lock" aria-hidden="true"></i></div>
            <h2 id="login-title">Selamat datang</h2>
            <p class="form-description">Masuk untuk mengakses dashboard akademik.</p>
            <?php if (session()->getFlashdata('error')): ?>
                <div class="error-message" role="alert"><i class="fas fa-exclamation-circle" aria-hidden="true"></i> <?= esc(session()->getFlashdata('error')) ?></div>
            <?php endif ?>
            <form action="<?= site_url('login') ?>" method="post">
                <?= csrf_field() ?>
                <label for="email">Email</label>
                <div class="input-wrap"><i class="far fa-envelope" aria-hidden="true"></i><input id="email" name="email" type="email" value="<?= esc(session()->getFlashdata('email') ?? '', 'attr') ?>" placeholder="Masukkan email" autocomplete="username" required></div>
                <label for="password">Password</label>
                <div class="input-wrap"><i class="fas fa-lock" aria-hidden="true"></i><input id="password" name="password" type="password" placeholder="Masukkan password" autocomplete="current-password" required></div>
                <button class="login-button" type="submit">Login <i class="fas fa-arrow-right" aria-hidden="true"></i></button>
                <button class="demo-button" type="button" id="fill-demo"><i class="far fa-user" aria-hidden="true"></i> Isi akun demo</button>
            </form>
            <div class="demo-note"><i class="fas fa-info-circle" aria-hidden="true"></i><p><strong>Akun simulasi</strong><br>Siapa pun dapat menggunakan akun demo untuk mengakses dashboard.</p></div>
        </div>
        <p class="login-footer">© <?= date('Y') ?> Akademika · PBKK</p>
    </section>
</main>
<script>
    document.getElementById('fill-demo').addEventListener('click', function () {
        document.getElementById('email').value = <?= json_encode(\Config\DemoAdmin::EMAIL, JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT) ?>;
        document.getElementById('password').value = <?= json_encode(\Config\DemoAdmin::PASSWORD, JSON_HEX_TAG | JSON_HEX_AMP | JSON_HEX_APOS | JSON_HEX_QUOT) ?>;
    });
</script>
</body>
</html>
