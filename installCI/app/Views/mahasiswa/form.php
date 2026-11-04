<?= $this->extend('layouts/admin') ?>
<?= $this->section('content') ?>
<h1 class="h3 mb-3"><?= $mahasiswa ? 'Edit' : 'Tambah' ?> mahasiswa</h1>
<?php if (session()->getFlashdata('error')): ?><div class="alert alert-danger"><?= esc(session()->getFlashdata('error')) ?></div><?php endif ?>
<?php $errors = session('_ci_validation_errors') ?? []; ?>
<?php if ($errors): ?><div class="alert alert-danger"><ul class="mb-0"><?php foreach ($errors as $error): ?><li><?= esc($error) ?></li><?php endforeach ?></ul></div><?php endif ?>
<form method="post" enctype="multipart/form-data" action="<?= $mahasiswa ? site_url('mahasiswa/' . $mahasiswa['id'] . '/update') : site_url('mahasiswa') ?>">
    <?= csrf_field() ?>
    <?php foreach (['nim' => 'NIM', 'nama' => 'Nama', 'email' => 'Email', 'jurusan' => 'Jurusan'] as $field => $label): ?>
        <div class="form-group"><label for="<?= $field ?>"><?= $label ?></label>
            <input class="form-control" id="<?= $field ?>" name="<?= $field ?>" type="<?= $field === 'email' ? 'email' : 'text' ?>" maxlength="<?= ['nim' => 30, 'nama' => 100, 'email' => 254, 'jurusan' => 100][$field] ?>" value="<?= esc(old($field, $mahasiswa[$field] ?? ''), 'attr') ?>" required>
        </div>
    <?php endforeach ?>
    <div class="form-group"><label for="jenis_kelamin">Jenis kelamin</label>
        <select class="form-control" id="jenis_kelamin" name="jenis_kelamin" required>
            <option value="">Pilih jenis kelamin</option>
            <?php foreach (['Laki-laki', 'Perempuan'] as $jenis): ?>
                <option value="<?= esc($jenis, 'attr') ?>" <?= old('jenis_kelamin', $mahasiswa['jenis_kelamin'] ?? '') === $jenis ? 'selected' : '' ?>><?= esc($jenis) ?></option>
            <?php endforeach ?>
        </select>
    </div>
    <?php if ($mahasiswa && $mahasiswa['foto']): ?><p><img src="<?= site_url('mahasiswa/' . $mahasiswa['id'] . '/photo') ?>" alt="Foto saat ini" width="100"></p><?php endif ?>
    <div class="form-group"><label for="foto">Foto (opsional; JPG, PNG, atau WebP, maks. 2 MB)</label><input class="form-control-file" type="file" id="foto" name="foto" accept="image/jpeg,image/png,image/webp"></div>
    <button class="btn btn-primary" type="submit">Simpan</button> <a class="btn btn-secondary" href="<?= site_url('mahasiswa') ?>">Batal</a>
</form>
<?= $this->endSection() ?>
