<?= $this->extend('layouts/admin') ?>
<?= $this->section('content') ?>
<div class="page-heading"><div><h1>Dashboard</h1></div><a class="btn btn-accent" href="<?= site_url('mahasiswa/create') ?>"><i class="fas fa-plus mr-2"></i>Tambah mahasiswa</a></div>
<div class="metrics">
    <div class="metric"><span class="metric-icon blue"><i class="fas fa-user-graduate"></i></span><div><span>Total mahasiswa</span><strong><?= $total ?></strong><small>Mahasiswa terdaftar</small></div></div>
    <div class="metric"><span class="metric-icon purple"><i class="fas fa-layer-group"></i></span><div><span>Jurusan</span><strong><?= $jurusan ?></strong><small>Jurusan terdaftar</small></div></div>
    <div class="metric"><span class="metric-icon cyan"><i class="fas fa-mars"></i></span><div><span>Laki-laki</span><strong><?= $lakiLaki ?></strong><small>Mahasiswa</small></div></div>
    <div class="metric"><span class="metric-icon pink"><i class="fas fa-venus"></i></span><div><span>Perempuan</span><strong><?= $perempuan ?></strong><small>Mahasiswa</small></div></div>
</div>
<div class="panel"><div class="panel-heading"><div><h2>Mahasiswa terbaru</h2><p>Lima data mahasiswa yang terakhir ditambahkan.</p></div><a class="text-link" href="<?= site_url('mahasiswa') ?>">Lihat semua <i class="fas fa-arrow-right ml-1"></i></a></div>
<?= view('mahasiswa/table', ['mahasiswa' => $mahasiswa, 'showActions' => false]) ?>
</div>
<?= $this->endSection() ?>
