<?php
namespace App\Controllers;

use Config\DemoAdmin;

class ShopAdminAuth extends BaseController
{
    public function login() { return view('admin_login'); }
    public function attempt() {
        $email = strtolower(trim((string) $this->request->getPost('email')));
        $password = (string) $this->request->getPost('password');
        if (!hash_equals(DemoAdmin::EMAIL, $email) || !hash_equals(DemoAdmin::PASSWORD, $password)) return redirect()->to(site_url('login'))->with('error', 'Email atau kata sandi salah.');
        session()->regenerate(true);
        session()->set(['role' => 'admin']);
        return redirect()->to(site_url('admin/shop'));
    }
    public function logout() { session()->remove('role'); session()->regenerate(true); return redirect()->to(site_url('/')); }
}
