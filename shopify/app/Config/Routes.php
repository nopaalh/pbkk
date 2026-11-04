<?php

use CodeIgniter\Router\RouteCollection;

/** @var RouteCollection $routes */
$routes->get('/', 'Home::index');
$routes->get('login', 'ShopAdminAuth::login');
$routes->post('login', 'ShopAdminAuth::attempt');
$routes->post('admin/logout', 'ShopAdminAuth::logout');

$routes->get('product/(:num)', 'Shop::product/$1');
$routes->get('cart', 'Shop::cart');
$routes->post('cart/add/(:num)', 'Shop::add/$1');
$routes->post('cart/update', 'Shop::updateCart');
$routes->get('checkout', 'Shop::checkout');
$routes->post('checkout', 'Shop::placeOrder');
$routes->get('invoice/(:segment)', 'Shop::invoice/$1');
$routes->get('account', 'Shop::account');
$routes->post('account/register', 'Shop::register');
$routes->post('account/login', 'Shop::login');
$routes->post('account/logout', 'Shop::logout');
$routes->get('profile', 'Shop::profile');
$routes->get('orders', 'Shop::orders');
$routes->get('admin/shop', 'Shop::adminDashboard');
$routes->get('admin/shop/products/new', 'Shop::productForm');
$routes->get('admin/shop/products/(:num)/edit', 'Shop::productForm/$1');
$routes->post('admin/shop/products', 'Shop::saveProduct');
$routes->post('admin/shop/products/(:num)', 'Shop::saveProduct/$1');
$routes->post('admin/shop/products/(:num)/delete', 'Shop::deleteProduct/$1');
$routes->post('admin/shop/orders/(:num)/status', 'Shop::status/$1');
