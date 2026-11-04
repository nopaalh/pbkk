<?= $this->extend('layouts/admin') ?>
<?= $this->section('content') ?>
<h1 class="h3 mb-3">Mahasiswa</h1>
<?php foreach (['success', 'error'] as $type): ?>
    <?php if (session()->getFlashdata($type)): ?>
        <div class="alert alert-<?= $type === 'error' ? 'danger' : 'success' ?>"><?= esc(session()->getFlashdata($type)) ?></div>
    <?php endif ?>
<?php endforeach ?>
<a class="btn btn-primary mb-3" href="<?= site_url('mahasiswa/create') ?>">Tambah mahasiswa</a>
<div class="table-responsive"><table class="table table-bordered bg-white">
    <thead><tr><th>Foto</th><th>NIM</th><th>Nama</th><th>Email</th><th>Jurusan</th><th>Jenis kelamin</th><th>Aksi</th></tr></thead>
    <tbody>
    <?php foreach ($mahasiswa as $row): ?>
        <tr>
            <td><?php if ($row['foto']): ?><img src="<?= site_url('mahasiswa/' . $row['id'] . '/photo') ?>" alt="Foto <?= esc($row['nama'], 'attr') ?>" width="60"><?php else: ?>—<?php endif ?></td>
            <td><?= esc($row['nim']) ?></td><td><?= esc($row['nama']) ?></td><td><?= esc($row['email']) ?></td><td><?= esc($row['jurusan']) ?></td><td><?= esc($row['jenis_kelamin']) ?></td>
            <td><a class="btn btn-sm btn-secondary" href="<?= site_url('mahasiswa/' . $row['id'] . '/edit') ?>">Edit</a>
                <form class="d-inline" method="post" action="<?= site_url('mahasiswa/' . $row['id'] . '/delete') ?>" onsubmit="return confirm('Hapus mahasiswa ini?')">
                    <?= csrf_field() ?><button class="btn btn-sm btn-danger" type="submit">Hapus</button>
                </form>
            </td>
        </tr>
    <?php endforeach ?>
    <?php if (! $mahasiswa): ?><tr><td colspan="7">Belum ada data mahasiswa.</td></tr><?php endif ?>
    </tbody>
</table></div>
<?= $this->endSection() ?>
