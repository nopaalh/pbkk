<?php
$directory = 'assets/images/';
$placeholder = base_url($directory . 'placeholder.svg');
$file = $image ?? '';
$hasPhoto = $file !== '' && is_file(FCPATH . $directory . $file);
?>
<div class="photo <?= $hasPhoto ? 'photo--framed photo--' . esc($frame, 'attr') : 'photo--missing' ?>">
    <img
        src="<?= esc($hasPhoto ? base_url($directory . $file) : $placeholder, 'attr') ?>"
        alt="<?= esc($hasPhoto ? ($description ?? '') : 'Gambar belum tersedia', 'attr') ?>"
        data-fallback="<?= esc($placeholder, 'attr') ?>"
        <?= isset($eager) && $eager ? 'fetchpriority="high"' : 'loading="lazy"' ?>
    >
</div>
