<?= $this->extend('layouts/main') ?>

<?= $this->section('content') ?>

<div class="card shadow-sm">
    <div class="card-body">
        <h1>About This Website</h1>

        <p>
            Website ini dibuat menggunakan CodeIgniter 4
            untuk mempelajari konsep MVC dan Layout Inheritance.
        </p>

        <a href="/profile" class="btn btn-primary">
            Back to Profile
        </a>
    </div>
</div>

<?= $this->endSection() ?>
