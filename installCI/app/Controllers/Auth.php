<?php

namespace App\Controllers;

use Config\DemoAdmin;

class Auth extends BaseController
{
    public function login()
    {
        return view('auth/login');
    }

    public function attempt()
    {
        $email = strtolower(trim((string) $this->request->getPost('email')));
        $password = (string) $this->request->getPost('password');
        if (! hash_equals(DemoAdmin::EMAIL, $email) || ! hash_equals(DemoAdmin::PASSWORD, $password)) {
            return redirect()->to('/login')->with('error', 'Email atau password salah.')->with('email', $email);
        }

        $session = session();
        $session->regenerate(true);
        $session->set(['user_id' => 1, 'user_email' => DemoAdmin::EMAIL, 'isLoggedIn' => true, 'role' => 'admin']);

        return redirect()->to('/dashboard');
    }

    public function logout()
    {
        session()->destroy();

        return redirect()->to('/login');
    }
}
