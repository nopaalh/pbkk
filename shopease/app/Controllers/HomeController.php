<?php

declare(strict_types=1);

namespace App\Controllers;

use Config\DemoCatalog;

final class HomeController extends BaseController
{
    public function index(): string
    {
        $catalog = config(DemoCatalog::class);
        $latest = $catalog->products;
        $bestsellers = $catalog->products;

        usort($latest, static fn (array $a, array $b): int =>
            strcmp($b['addedOn'], $a['addedOn']) ?: strcasecmp($a['name'], $b['name'])
        );
        usort($bestsellers, static fn (array $a, array $b): int =>
            ($b['sold'] <=> $a['sold']) ?: strcasecmp($a['name'], $b['name'])
        );

        return view('frontend/home', [
            'page' => 'home',
            'title' => 'Beranda',
            'categories' => $catalog->categories,
            'latest' => array_slice($latest, 0, 3),
            'bestsellers' => array_slice($bestsellers, 0, 3),
        ]);
    }

    public function about(): string
    {
        return view('frontend/about', ['page' => 'about', 'title' => 'Tentang']);
    }

    public function contact(): string
    {
        return view('frontend/contact', ['page' => 'contact', 'title' => 'Kontak']);
    }
}
