<?php

use CodeIgniter\Router\RouteCollection;

/** @var RouteCollection $routes */
$routes->get('/', 'Home::index');
$routes->get('/profile', 'Profile::index');
$routes->get('/about', 'Profile::about');
$routes->get('/login', 'Auth::login');
$routes->post('/login', 'Auth::attempt');

$routes->group('', ['filter' => 'auth'], static function ($routes) {
    $routes->get('dashboard', 'Dashboard::index');
    $routes->post('logout', 'Auth::logout');
    $routes->get('mahasiswa', 'Mahasiswa::index');
    $routes->get('mahasiswa/create', 'Mahasiswa::create');
    $routes->post('mahasiswa', 'Mahasiswa::store');
    $routes->get('mahasiswa/(:num)/edit', 'Mahasiswa::edit/$1');
    $routes->post('mahasiswa/(:num)/update', 'Mahasiswa::update/$1');
    $routes->post('mahasiswa/(:num)/delete', 'Mahasiswa::delete/$1');
    $routes->get('mahasiswa/(:num)/photo', 'Mahasiswa::photo/$1');
});
