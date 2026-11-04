<?php
/** @var \CodeIgniter\View\View $this */
?>

<?= $this->extend('layouts/main') ?>

<?= $this->section('content') ?>

<div class="card shadow-sm">
    <div class="card-body">
        <h1>Welcome to My Profile!</h1>

        <hr>

        <p>Nama: Naufal</p>
        <p>Jurusan: Teknik Informatika</p>
        <p>Universitas: ITS</p>
    </div>
</div>

<?= $this->endSection() ?>
