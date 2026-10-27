// Publicidad: panel de la campana y ampliación flotante de la imagen.
(function () {
    var st = document.createElement('style');
    st.textContent =
        '[hidden]{display:none!important}' +
        '.up-notif-btn{position:relative;cursor:pointer}' +
        '.up-notif-panel{position:absolute;right:0;top:calc(100% + 10px);width:300px;max-height:420px;overflow-y:auto;z-index:9000;background:#0f1222;border:1px solid #232a44;border-radius:14px;box-shadow:0 18px 40px rgba(0,0,0,.55);padding:10px;display:grid;gap:8px;text-align:left}' +
        '.up-notif-head{font-weight:700;color:#e5e7eb;font-size:.9rem;padding:2px 4px}' +
        '.up-notif-vacio{color:#94a3b8;font-size:.85rem;padding:10px;text-align:center}' +
        '.up-notif-item{display:grid;gap:6px;padding:6px;border-radius:10px;border:1px solid #232a44;background:rgba(255,255,255,.03);color:#e5e7eb;cursor:zoom-in;text-align:left}' +
        '.up-notif-item img{width:100%;height:110px;object-fit:cover;border-radius:8px;display:block}' +
        '.up-notif-item span{font-size:.85rem;font-weight:600;white-space:nowrap;overflow:hidden;text-overflow:ellipsis}' +
        '#an-lightbox{position:fixed;inset:0;z-index:10000;background:rgba(0,0,0,.82);display:flex;align-items:center;justify-content:center;padding:20px}' +
        '#an-lightbox .an-box{position:relative;max-width:min(92vw,900px);max-height:90vh;display:flex;flex-direction:column;gap:10px;align-items:center;animation:an-zoom .25s ease}' +
        '#an-lightbox img{max-width:100%;max-height:78vh;object-fit:contain;border-radius:14px;box-shadow:0 20px 60px rgba(0,0,0,.6)}' +
        '#an-lightbox .an-tit{color:#fff;font-weight:600;text-align:center}' +
        '#an-lightbox .an-x{position:absolute;top:-12px;right:-12px;width:36px;height:36px;border-radius:50%;border:none;background:#fff;color:#000;font-size:1rem;cursor:pointer}' +
        '#an-lightbox .an-go{padding:9px 16px;border-radius:10px;background:linear-gradient(135deg,#22d3ee,#a78bfa);color:#04121a;font-weight:600;text-decoration:none}' +
        '@keyframes an-zoom{from{transform:scale(.85);opacity:0}to{transform:scale(1);opacity:1}}';
    document.head.appendChild(st);

    function cerrarZoom() {
        var o = document.getElementById('an-lightbox');
        if (o) o.remove();
    }

    function ampliar(src, titulo, enlace) {
        cerrarZoom();
        var o = document.createElement('div');
        o.id = 'an-lightbox';
        var caja = document.createElement('div');
        caja.className = 'an-box';
        var x = document.createElement('button');
        x.type = 'button';
        x.className = 'an-x';
        x.setAttribute('aria-label', 'Cerrar');
        x.textContent = '✕';
        var img = document.createElement('img');
        img.src = src;
        img.alt = titulo || '';
        var t = document.createElement('div');
        t.className = 'an-tit';
        t.textContent = titulo || '';
        caja.appendChild(x);
        caja.appendChild(img);
        caja.appendChild(t);
        if (enlace) {
            var a = document.createElement('a');
            a.href = enlace;
            a.target = '_blank';
            a.rel = 'noopener noreferrer';
            a.className = 'an-go';
            a.textContent = 'Abrir enlace';
            caja.appendChild(a);
        }
        o.appendChild(caja);
        o.addEventListener('click', function (e) {
            if (e.target === o || e.target === x) cerrarZoom();
        });
        document.body.appendChild(o);
    }

    // Cualquier elemento con data-zoom se amplía al tocarlo (panel de la campana y anuncios de Inicio)
    document.addEventListener('click', function (e) {
        var b = e.target.closest('[data-zoom]');
        if (!b) return;
        e.preventDefault();
        ampliar(b.dataset.zoom, b.dataset.titulo, b.dataset.enlace);
    });
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') cerrarZoom();
    });

    // Campana: abre y cierra el panel de publicidad
    var btn = document.getElementById('upNotifBtn');
    var panel = document.getElementById('up-notif-panel');
    if (btn && panel) {
        btn.addEventListener('click', function (e) {
            if (panel.contains(e.target)) return;
            e.stopPropagation();
            panel.hidden = !panel.hidden;
        });
        document.addEventListener('click', function (e) {
            if (!btn.contains(e.target)) panel.hidden = true;
        });
    }
})();
