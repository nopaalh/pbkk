<?php

use CodeIgniter\Router\RouteCollection;

/** @var RouteCollection $routes */
$routes->get('/', 'Home::index');
$routes->get('/profile', 'Profile::index');
$routes->get('/about', 'Profile::about');
