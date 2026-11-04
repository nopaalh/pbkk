<?= $this->extend('layouts/admin') ?>
<?= $this->section('content') ?>
<div class="page-heading"><div><h1>Mahasiswa</h1></div><a class="btn btn-accent" href="<?= site_url('mahasiswa/create') ?>"><i class="fas fa-plus mr-2"></i>Tambah mahasiswa</a></div>
<?php foreach (['success', 'error'] as $type): ?><?php if (session()->getFlashdata($type)): ?><div class="alert alert-<?= $type === 'error' ? 'danger' : 'success' ?>"><?= esc(session()->getFlashdata($type)) ?></div><?php endif ?><?php endforeach ?>
<div class="panel"><div class="panel-heading"><div><h2>Daftar mahasiswa</h2><p><?= count($mahasiswa) ?> mahasiswa terdaftar</p></div></div><?= view('mahasiswa/table', ['mahasiswa' => $mahasiswa, 'showActions' => true]) ?></div>
<?= $this->endSection() ?>
