<?= $this->extend('layouts/admin') ?>
<?= $this->section('content') ?>
<div class="page-heading"><div><div class="page-kicker"><a href="<?= site_url('mahasiswa') ?>">Mahasiswa</a> / <?= $mahasiswa ? 'Edit' : 'Tambah' ?></div><h1><?= $mahasiswa ? 'Edit' : 'Tambah' ?> mahasiswa</h1><p>Lengkapi informasi mahasiswa di bawah ini.</p></div></div>
<?php if (session()->getFlashdata('error')): ?><div class="alert alert-danger"><?= esc(session()->getFlashdata('error')) ?></div><?php endif ?>
<?php $errors = session('_ci_validation_errors') ?? []; ?>
<?php if ($errors): ?><div class="alert alert-danger"><ul class="mb-0"><?php foreach ($errors as $error): ?><li><?= esc($error) ?></li><?php endforeach ?></ul></div><?php endif ?>
<form class="panel student-form" method="post" enctype="multipart/form-data" action="<?= $mahasiswa ? site_url('mahasiswa/' . $mahasiswa['id'] . '/update') : site_url('mahasiswa') ?>">
<?= csrf_field() ?><div class="form-heading"><h2>Informasi mahasiswa</h2><p>Data ini akan ditampilkan pada daftar mahasiswa.</p></div>
<div class="form-layout"><div class="form-fields"><?php foreach (['nim' => 'NIM', 'nama' => 'Nama lengkap', 'email' => 'Email', 'jurusan' => 'Jurusan'] as $field => $label): ?><div class="form-group"><label for="<?= $field ?>"><?= $label ?></label><input class="form-control" id="<?= $field ?>" name="<?= $field ?>" type="<?= $field === 'email' ? 'email' : 'text' ?>" maxlength="<?= ['nim' => 30, 'nama' => 100, 'email' => 254, 'jurusan' => 100][$field] ?>" value="<?= esc(old($field, $mahasiswa[$field] ?? ''), 'attr') ?>" required></div><?php endforeach ?>
<div class="form-group"><label for="jenis_kelamin">Jenis kelamin</label><select class="form-control" id="jenis_kelamin" name="jenis_kelamin" required><option value="">Pilih jenis kelamin</option><?php foreach (['Laki-laki', 'Perempuan'] as $jenis): ?><option value="<?= esc($jenis, 'attr') ?>" <?= old('jenis_kelamin', $mahasiswa['jenis_kelamin'] ?? '') === $jenis ? 'selected' : '' ?>><?= esc($jenis) ?></option><?php endforeach ?></select></div></div>
<div class="photo-field"><label for="foto">Foto mahasiswa</label><div class="photo-preview"><img id="photo-preview" <?= $mahasiswa && $mahasiswa['foto'] ? 'src="' . site_url('mahasiswa/' . $mahasiswa['id'] . '/photo') . '"' : 'hidden' ?> alt="Pratinjau foto mahasiswa"><span id="photo-placeholder" <?= $mahasiswa && $mahasiswa['foto'] ? 'hidden' : '' ?>><i class="fas fa-user"></i></span></div><label class="upload-button" for="foto"><i class="fas fa-upload mr-2"></i><?= $mahasiswa && $mahasiswa['foto'] ? 'Ganti foto' : 'Pilih foto' ?></label><input class="sr-only" type="file" id="foto" name="foto" accept="image/jpeg,image/png,image/webp"><p>Opsional. JPG, PNG, atau WebP<br>maksimal 2 MB.</p><?php if ($mahasiswa && $mahasiswa['foto']): ?><small>Foto lama tetap digunakan jika tidak diganti.</small><?php endif ?></div></div>
<div class="form-actions"><a class="btn btn-light" href="<?= site_url('mahasiswa') ?>">Batal</a><button class="btn btn-accent" type="submit">Simpan mahasiswa</button></div></form>
<?= $this->endSection() ?>
<?= $this->section('scripts') ?>
<script>
const input = document.getElementById('foto');
const preview = document.getElementById('photo-preview');
const placeholder = document.getElementById('photo-placeholder');
const original = preview.getAttribute('src');
let objectUrl;
input.addEventListener('change', () => {
    if (objectUrl) URL.revokeObjectURL(objectUrl);
    const file = input.files[0];
    objectUrl = file ? URL.createObjectURL(file) : null;
    const src = objectUrl || original;
    if (src) preview.src = src; else preview.removeAttribute('src');
    preview.hidden = !src;
    placeholder.hidden = !!src;
});
</script>
<?= $this->endSection() ?>
