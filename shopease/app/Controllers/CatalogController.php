<?php

declare(strict_types=1);

namespace App\Controllers;

use Config\DemoCatalog;

final class CatalogController extends BaseController
{
    public function index(): string
    {
        $catalog = config(DemoCatalog::class);
        $productsByCategory = [];

        foreach ($catalog->products as $product) {
            $productsByCategory[$product['category']][] = $product;
        }

        return view('frontend/catalog', [
            'page' => 'catalog',
            'title' => 'Katalog',
            'categories' => $catalog->categories,
            'productsByCategory' => $productsByCategory,
        ]);
    }
}
