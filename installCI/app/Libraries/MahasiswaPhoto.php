<?php

namespace App\Libraries;

use CodeIgniter\HTTP\Files\UploadedFile;
use RuntimeException;

class MahasiswaPhoto
{
    private string $directory;

    public function __construct(?string $directory = null)
    {
        $this->directory = $directory ?? WRITEPATH . 'uploads/mahasiswa';
    }

    public function store(UploadedFile $file): string
    {
        if (! is_dir($this->directory) && ! mkdir($this->directory, 0700, true) && ! is_dir($this->directory)) {
            throw new RuntimeException('Direktori foto tidak dapat dibuat.');
        }

        $extensions = ['image/jpeg' => 'jpg', 'image/png' => 'png', 'image/webp' => 'webp'];
        $mime = $file->getMimeType();
        if (! $file->isValid() || ! isset($extensions[$mime]) || @getimagesize($file->getTempName()) === false) {
            throw new RuntimeException('Foto tidak valid.');
        }
        $name = bin2hex(random_bytes(16)) . '.' . $extensions[$mime];
        $file->move($this->directory, $name);

        return $name;
    }

    public function path(?string $name): ?string
    {
        if ($name === null || ! preg_match('/\A[a-f0-9]{32}\.(jpg|png|webp)\z/', $name)) {
            return null;
        }
        $dir = realpath($this->directory);
        $path = realpath($this->directory . DIRECTORY_SEPARATOR . $name);
        if ($dir === false || $path === false || dirname($path) !== $dir || ! is_file($path) || is_link($this->directory . DIRECTORY_SEPARATOR . $name)) {
            return null;
        }

        return $path;
    }

    public function delete(?string $name): void
    {
        $path = $this->path($name);
        if ($path !== null && ! unlink($path)) {
            log_message('error', 'Gagal menghapus foto mahasiswa: {name}', ['name' => $name]);
        }
    }
}
