<?php

declare(strict_types=1);

namespace Config;

use CodeIgniter\Config\BaseConfig;

final class DemoCatalog extends BaseConfig
{
    public array $categories = [
        'elektronik' => ['name' => 'Elektronik', 'description' => 'Audio dan perangkat praktis'],
        'fashion' => ['name' => 'Fashion', 'description' => 'Paduan santai untuk keseharian'],
        'aksesori' => ['name' => 'Aksesori', 'description' => 'Pemanis gaya dan pelengkap aktivitas'],
    ];

    public array $products = [
        [
            'id' => 'headphone-studio',
            'name' => 'Headphone Studio',
            'category' => 'elektronik',
            'price' => 429000,
            'addedOn' => '2026-11-22',
            'sold' => 58,
            'image' => 'collection-studio.png',
            'frame' => 'top-right',
        ],
        [
            'id' => 'speaker-mini',
            'name' => 'Speaker Mini',
            'category' => 'elektronik',
            'price' => 249000,
            'addedOn' => '2026-11-09',
            'sold' => 86,
            'image' => 'collection-essentials.png',
            'frame' => 'top-left',
        ],
        [
            'id' => 'tote-kanvas',
            'name' => 'Tote Kanvas',
            'category' => 'fashion',
            'price' => 129000,
            'addedOn' => '2026-11-20',
            'sold' => 102,
            'image' => 'collection-studio.png',
            'frame' => 'bottom-left',
        ],
        [
            'id' => 'topi-weekend',
            'name' => 'Topi Weekend',
            'category' => 'fashion',
            'price' => 89000,
            'addedOn' => '2026-11-14',
            'sold' => 39,
            'image' => 'collection-essentials.png',
            'frame' => 'top-right',
        ],
        [
            'id' => 'jam-tempo',
            'name' => 'Jam Pintar Tempo',
            'category' => 'aksesori',
            'price' => 549000,
            'addedOn' => '2026-11-18',
            'sold' => 47,
            'image' => 'collection-studio.png',
            'frame' => 'bottom-right',
        ],
        [
            'id' => 'kacamata-senja',
            'name' => 'Kacamata Senja',
            'category' => 'aksesori',
            'price' => 159000,
            'addedOn' => '2026-11-12',
            'sold' => 73,
            'image' => 'collection-essentials.png',
            'frame' => 'bottom-left',
        ],
        [
            'id' => 'pouch-rute',
            'name' => 'Pouch Rute',
            'category' => 'aksesori',
            'price' => 79000,
            'addedOn' => '2026-11-10',
            'sold' => 31,
            'image' => 'collection-essentials.png',
            'frame' => 'bottom-right',
        ],
    ];
}
