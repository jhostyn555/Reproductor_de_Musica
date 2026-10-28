// Service worker de RolaLoka.
// - Precarga solo lo estático (iconos, manifiesto, página sin conexión).
// - Estáticos (css, js, iconos): sirve desde caché y actualiza en segundo plano.
// - Navegación: siempre red; si no hay conexión, muestra la página sin conexión.
// - Audio y cualquier origen externo (Cloudinary): no se intercepta.
const VERSION = 'musicapp-v1';
const PRECACHE = [
    '/offline.html',
    '/manifest.json',
    '/icons/icon-192.png',
    '/icons/icon-512.png',
    '/icons/apple-touch-icon-180.png'
];

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open(VERSION)
            .then((cache) => cache.addAll(PRECACHE))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (event) => {
    event.waitUntil(
        caches.keys()
            .then((keys) => Promise.all(keys.filter((k) => k !== VERSION).map((k) => caches.delete(k))))
            .then(() => self.clients.claim())
    );
});

self.addEventListener('fetch', (event) => {
    const req = event.request;
    if (req.method !== 'GET') return;

    const url = new URL(req.url);
    if (url.origin !== self.location.origin) return;   // audio de Cloudinary y otros orígenes: sin tocar

    if (req.mode === 'navigate') {
        event.respondWith(fetch(req).catch(() => caches.match('/offline.html')));
        return;
    }

    const esEstatico = /^\/(css|js|lib|icons)\//.test(url.pathname) ||
        /\.(png|jpe?g|webp|svg|ico|woff2?)$/.test(url.pathname);
    if (esEstatico) {
        event.respondWith(
            caches.open(VERSION).then((cache) =>
                cache.match(req).then((cached) => {
                    const red = fetch(req).then((res) => {
                        if (res.ok) cache.put(req, res.clone());
                        return res;
                    });
                    return cached || red;
                })
            )
        );
    }
});
