<?php

use CodeIgniter\Router\RouteCollection;

/** @var RouteCollection $routes */
$routes->get('/', 'HomeController::index', ['as' => 'home']);
$routes->get('katalog', 'CatalogController::index', ['as' => 'catalog']);
$routes->get('about', 'HomeController::about', ['as' => 'about']);
$routes->get('contact', 'HomeController::contact', ['as' => 'contact']);
