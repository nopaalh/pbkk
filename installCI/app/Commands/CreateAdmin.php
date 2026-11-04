<?php

namespace App\Commands;

use App\Models\UserModel;
use CodeIgniter\CLI\BaseCommand;
use CodeIgniter\CLI\CLI;
use Throwable;

class CreateAdmin extends BaseCommand
{
    protected $group = 'Auth';
    protected $name = 'admin:create';
    protected $description = 'Membuat akun admin secara interaktif.';

    public function run(array $params)
    {
        if (! function_exists('stream_isatty') || ! stream_isatty(STDIN) || ! function_exists('exec')) {
            CLI::error('Terminal interaktif dengan dukungan stty diperlukan untuk memasukkan password secara aman.');
            return EXIT_ERROR;
        }

        $email = strtolower(trim(CLI::prompt('Email')));
        if (strlen($email) > 254 || ! filter_var($email, FILTER_VALIDATE_EMAIL)) {
            CLI::error('Email tidak valid.');
            return EXIT_ERROR;
        }

        exec('stty -echo 2>/dev/null', $output, $status);
        if ($status !== 0) {
            CLI::error('Tidak dapat menonaktifkan echo terminal.');
            return EXIT_ERROR;
        }

        try {
            fwrite(STDOUT, 'Password: ');
            $password = fgets(STDIN);
            fwrite(STDOUT, "\nKonfirmasi password: ");
            $confirmation = fgets(STDIN);
            fwrite(STDOUT, "\n");
        } finally {
            exec('stty echo 2>/dev/null');
        }

        if ($password === false || $confirmation === false) {
            CLI::error('Password tidak dapat dibaca.');
            return EXIT_ERROR;
        }
        $password = rtrim($password, "\r\n");
        $confirmation = rtrim($confirmation, "\r\n");
        if (strlen($password) < 8 || strlen($password) > 72 || str_contains($password, "\0") || $password !== $confirmation) {
            CLI::error('Password harus cocok dan berisi 8–72 byte tanpa karakter NUL.');
            return EXIT_ERROR;
        }

        try {
            $users = new UserModel();
            if ($users->where('email', $email)->first()) {
                CLI::error('Email sudah terdaftar.');
                return EXIT_ERROR;
            }
            if (! $users->insert(['email' => $email, 'password_hash' => password_hash($password, PASSWORD_DEFAULT)])) {
                CLI::error('Gagal membuat admin.');
                return EXIT_ERROR;
            }
        } catch (Throwable $e) {
            log_message('error', 'Gagal membuat admin: {error}', ['error' => $e->getMessage()]);
            CLI::error('Gagal membuat admin. Periksa konfigurasi database dan log aplikasi.');
            return EXIT_ERROR;
        }

        CLI::write('Admin berhasil dibuat.', 'green');
        return EXIT_SUCCESS;
    }
}
