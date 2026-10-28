// Ventana flotante "Agregar a playlist" (varias playlists o crear una nueva con la canción)
// y "Ir al álbum" (solo si el artista tiene álbum).
(function () {
    var win = document.getElementById('pf-flotante');
    if (!win) return;

    var head = document.getElementById('pf-head');
    var cuerpo = document.getElementById('pf-cuerpo');
    var lista = document.getElementById('pf-lista');
    var cont = document.getElementById('pf-cont');
    var msg = document.getElementById('pf-msg');
    var nuevoNombre = document.getElementById('pf-nueva-nombre');
    var btnMin = document.getElementById('pf-min');
    var btnCerrar = document.getElementById('pf-cerrar');
    var btnCrear = document.getElementById('pf-crear');
    var btnAgregar = document.getElementById('pf-agregar');
    var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');

    var musicaId = null;
    var expandida = false;

    function token() { return tokenEl ? tokenEl.value : ''; }

    function cerrarMenus() {
        document.querySelectorAll('[data-mm-pop]').forEach(function (p) { p.hidden = true; });
    }

    function toast(texto) {
        var d = document.createElement('div');
        d.className = 'pf-toast';
        d.textContent = texto;
        document.body.appendChild(d);
        setTimeout(function () { d.remove(); }, 3200);
    }

    function setExpandida(v) {
        expandida = v;
        cuerpo.hidden = !v;
        btnMin.textContent = v ? '▾' : '▴';
        head.setAttribute('aria-expanded', String(v));
    }

    function mensaje(texto, ok) {
        msg.textContent = texto;
        msg.className = 'pf-msg ' + (ok ? 'ok' : 'err');
    }

    function seleccionadas() {
        return Array.prototype.slice.call(lista.querySelectorAll('input[type="checkbox"]:checked:not(:disabled)'))
            .map(function (c) { return Number(c.value); });
    }

    function actualizarCont() {
        var n = seleccionadas().length;
        cont.textContent = n ? n + ' seleccionada' + (n > 1 ? 's' : '') : '';
    }

    function cargar() {
        lista.innerHTML = '<p class="pf-vacio">Cargando…</p>';
        fetch('/MisPlaylists/Mis?musicaId=' + musicaId, { credentials: 'same-origin' })
            .then(function (r) {
                if (r.status === 401) { window.location.href = '/Account/Login'; throw new Error('sin sesión'); }
                if (!r.ok) throw new Error('error');
                return r.json();
            })
            .then(function (items) {
                lista.innerHTML = '';
                if (!items.length) {
                    lista.innerHTML = '<p class="pf-vacio">No tienes playlists. Crea una abajo.</p>';
                }
                items.forEach(function (p) {
                    var lab = document.createElement('label');
                    lab.className = 'pf-item';
                    var cb = document.createElement('input');
                    cb.type = 'checkbox';
                    cb.value = p.id;
                    cb.checked = p.contiene;
                    cb.disabled = p.contiene;
                    cb.addEventListener('change', actualizarCont);
                    var nm = document.createElement('span');
                    nm.textContent = p.nombre;
                    var sub = document.createElement('small');
                    sub.textContent = p.contiene ? 'Ya tiene esta canción' : p.total + (p.total === 1 ? ' canción' : ' canciones');
                    lab.appendChild(cb);
                    lab.appendChild(nm);
                    lab.appendChild(sub);
                    lista.appendChild(lab);
                });
                actualizarCont();
            })
            .catch(function (e) {
                if (e.message !== 'sin sesión') lista.innerHTML = '<p class="pf-vacio">No se pudieron cargar tus playlists.</p>';
            });
    }

    function abrir(id) {
        musicaId = id;
        msg.textContent = '';
        nuevoNombre.value = '';
        win.hidden = false;
        setExpandida(false);          // aparece minimizada; se expande al tocarla
        cargar();
    }

    // Agregar a varias playlists seleccionadas
    btnAgregar.addEventListener('click', function () {
        var ids = seleccionadas();
        if (!ids.length) { mensaje('Selecciona al menos una playlist.', false); return; }
        var body = new URLSearchParams();
        body.append('musicaId', musicaId);
        ids.forEach(function (id) { body.append('playlistIds', id); });
        fetch('/MisPlaylists/AgregarCancion', {
            method: 'POST', credentials: 'same-origin',
            headers: { 'RequestVerificationToken': token() },
            body: body
        })
        .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
        .then(function (d) {
            mensaje(d.agregadas ? 'Canción agregada a ' + d.agregadas + ' playlist(s).' : 'Ya estaba en esas playlists.', true);
            cargar();
        })
        .catch(function () { mensaje('No se pudo agregar la canción.', false); });
    });

    // Crear una playlist nueva que ya contiene la canción
    btnCrear.addEventListener('click', function () {
        var nombre = nuevoNombre.value.trim();
        if (!nombre) { mensaje('Escribe un nombre para la playlist.', false); return; }
        var body = new URLSearchParams();
        body.append('nombre', nombre);
        body.append('musicaId', musicaId);
        fetch('/MisPlaylists/CrearConCancion', {
            method: 'POST', credentials: 'same-origin',
            headers: { 'RequestVerificationToken': token() },
            body: body
        })
        .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
        .then(function (d) {
            nuevoNombre.value = '';
            mensaje('Playlist "' + d.nombre + '" creada con la canción.', true);
            cargar();
        })
        .catch(function () { mensaje('No se pudo crear la playlist.', false); });
    });

    nuevoNombre.addEventListener('keydown', function (e) {
        if (e.key === 'Enter') { e.preventDefault(); btnCrear.click(); }
    });

    head.addEventListener('click', function (e) {
        if (e.target === btnCerrar) return;
        setExpandida(!expandida);
    });
    head.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); setExpandida(!expandida); }
    });
    btnCerrar.addEventListener('click', function (e) {
        e.stopPropagation();
        win.hidden = true;
    });

    document.addEventListener('click', function (e) {
        // Abrir desde el menú de tres puntos
        var ap = e.target.closest('[data-agregar-playlist]');
        if (ap) {
            e.preventDefault();
            cerrarMenus();
            abrir(Number(ap.dataset.musicaId));
            return;
        }

        // Ir al álbum: solo si el artista tiene álbum
        var ia = e.target.closest('[data-ir-album]');
        if (ia) {
            e.preventDefault();
            cerrarMenus();
            fetch('/User/AlbumDelArtista?artistaId=' + encodeURIComponent(ia.dataset.artistaId), { credentials: 'same-origin' })
                .then(function (r) { if (!r.ok) throw new Error(); return r.json(); })
                .then(function (d) {
                    if (d.albumId) window.location.href = '/User/Album/' + d.albumId;
                    else toast('Este artista no tiene álbum.');
                })
                .catch(function () { toast('No se pudo verificar el álbum.'); });
        }
    });
})();
