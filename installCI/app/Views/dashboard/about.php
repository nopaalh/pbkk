<?= $this->extend('layouts/admin') ?>
<?= $this->section('content') ?>
<div class="page-heading"><div><div class="page-kicker">Informasi</div><h1>About</h1><p>Mengenal proyek ini.</p></div></div>
<div class="panel info-panel"><span class="metric-icon blue"><i class="fas fa-code"></i></span><div><h2>Tentang Sistem Akademik</h2><p>Website ini dibuat menggunakan CodeIgniter 4 untuk mempelajari konsep MVC dan layout inheritance. Dashboard menyediakan pengelolaan data mahasiswa dan foto dalam satu tempat.</p><a class="text-link" href="<?= site_url('dashboard/profile') ?>">Lihat profile <i class="fas fa-arrow-right ml-1"></i></a></div></div>
<?= $this->endSection() ?>
