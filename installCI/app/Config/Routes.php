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
});
