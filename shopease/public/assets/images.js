'use strict';

function showFallback(image) {
    const fallback = image.dataset.fallback;
    if (image.src === fallback) return;

    image.parentElement.classList.remove('photo--framed', 'photo--top-left', 'photo--top-right', 'photo--bottom-left', 'photo--bottom-right');
    image.parentElement.classList.add('photo--missing');
    image.alt = 'Gambar belum tersedia';
    image.src = fallback;
}

document.addEventListener('error', (event) => {
    if (event.target instanceof HTMLImageElement && event.target.dataset.fallback) {
        showFallback(event.target);
    }
}, true);

document.querySelectorAll('img[data-fallback]').forEach((image) => {
    if (image.complete && !image.naturalWidth) showFallback(image);
});
