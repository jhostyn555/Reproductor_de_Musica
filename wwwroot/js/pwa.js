// Registro del service worker y botón "Instalar app" (cuando el navegador lo permite).
(function () {
    if ('serviceWorker' in navigator) {
        window.addEventListener('load', function () {
            navigator.serviceWorker.register('/service-worker.js').catch(function () { });
        });
    }

    var evento = null;

    function botonInstalar() {
        var b = document.getElementById('pwa-install');
        if (b) return b;
        b = document.createElement('button');
        b.id = 'pwa-install';
        b.type = 'button';
        b.textContent = '📲 Instalar app';
        b.style.cssText = 'position:fixed;right:16px;bottom:16px;z-index:9999;padding:10px 16px;border:none;border-radius:999px;font-weight:600;color:#04121a;background:linear-gradient(135deg,#22d3ee,#a78bfa);box-shadow:0 8px 24px rgba(0,0,0,.35);cursor:pointer';
        b.hidden = true;
        b.addEventListener('click', function () {
            if (!evento) return;
            evento.prompt();
            evento.userChoice.then(function () { evento = null; b.hidden = true; });
        });
        document.body.appendChild(b);
        return b;
    }

    window.addEventListener('beforeinstallprompt', function (e) {
        e.preventDefault();
        evento = e;
        botonInstalar().hidden = false;
    });

    window.addEventListener('appinstalled', function () {
        evento = null;
        var b = document.getElementById('pwa-install');
        if (b) b.hidden = true;
    });
})();
