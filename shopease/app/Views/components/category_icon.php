<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.65" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
    <?php if ($category === 'elektronik'): ?>
        <path d="M4 14v-2a8 8 0 0 1 16 0v2M4 14a2 2 0 0 1 2-2h2v8H6a2 2 0 0 1-2-2zm16 0a2 2 0 0 0-2-2h-2v8h2a2 2 0 0 0 2-2z"/>
    <?php elseif ($category === 'fashion'): ?>
        <path d="M8 4 3 7l2.5 5L8 11v9h8v-9l2.5 1L21 7l-5-3a4 4 0 0 1-8 0Z"/>
    <?php else: ?>
        <path d="M3 12h3m12 0h3M5 12l1 6h4l2-4 2 4h4l1-6zM6 12l1-5m11 5-1-5"/>
    <?php endif ?>
</svg>
