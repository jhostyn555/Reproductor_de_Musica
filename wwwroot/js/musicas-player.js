// Reproductor de la página de músicas (Inicio y Músicas).
// - Cola (Queue, FIFO): la lista de canciones en el orden en que se muestran. Al terminar una, sigue la siguiente.
// - Pila (Stack, LIFO): historial de canciones reproducidas. "Anterior" desapila.
// - 👍 guarda la canción como favorita. Los comentarios se publican por canción.
// - Sin sesión, al intentar escuchar se muestra el inicio de sesión.
(function () {
    var root = document.getElementById('mu-player');
    if (!root) return;

    var secuencia = [];
    try { secuencia = JSON.parse(root.dataset.secuencia || '[]'); } catch (e) { secuencia = []; }
    var autenticado = root.dataset.autenticado === 'true';
    var urlLogin = root.dataset.urlLogin;
    var urlMeGusta = root.dataset.urlMegusta;
    var urlHistorial = root.dataset.urlHistorial;
    var urlComentarios = root.dataset.urlComentarios;
    var urlComentar = root.dataset.urlComentar;
    var tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');

    function $(id) { return document.getElementById(id); }
    function token() { return tokenEl ? tokenEl.value : ''; }

    var audio = new Audio();
    var idx = -1;     // posición actual en la cola
    var pila = [];    // Stack: posiciones reproducidas antes

    var miniImg = $('mp-mini-img'), miniTitulo = $('mp-mini-title'), miniArtista = $('mp-mini-artist');
    var miniToggle = $('mp-mini-toggle'), miniPrev = $('mp-mini-prev'), miniNext = $('mp-mini-next');
    var miniLike = $('mp-mini-like'), miniFill = $('mp-mini-fill'), miniBody = $('mp-mini-body');
    var fullImg = $('mp-full-img'), fullTitulo = $('mp-full-title'), fullArtista = $('mp-full-artist');
    var fullToggle = $('mp-full-toggle'), fullPrev = $('mp-full-prev'), fullNext = $('mp-full-next');
    var fullLike = $('mp-full-like'), seek = $('mp-seek'), curEl = $('mp-cur'), durEl = $('mp-dur');
    var cerrar = $('mp-close');
    var comBtn = $('mp-com-btn'), comPanel = $('mp-com'), comCerrar = $('mp-com-cerrar');
    var comLista = $('mp-com-lista'), comForm = $('mp-com-form'), comTexto = $('mp-com-texto');

    function actual() { return idx >= 0 ? secuencia[idx] : null; }

    function mmss(s) {
        if (!isFinite(s) || s <= 0) return '0:00';
        var m = Math.floor(s / 60), r = Math.floor(s % 60);
        return m + ':' + (r < 10 ? '0' : '') + r;
    }

    function pintar() {
        var t = actual();
        root.hidden = !t;
        if (!t) return;
        miniImg.src = t.imagen || '';
        fullImg.src = t.imagen || '';
        miniTitulo.textContent = t.nombre;
        fullTitulo.textContent = t.nombre;
        miniArtista.textContent = t.artista;
        fullArtista.textContent = t.artista;
        var icono = audio.paused ? '▶' : '⏸';
        miniToggle.textContent = icono;
        fullToggle.textContent = icono;
        [miniLike, fullLike].forEach(function (b) {
            b.classList.toggle('is-liked', !!t.meGusta);
            b.setAttribute('aria-pressed', String(!!t.meGusta));
        });
    }

    // Guarda en el historial del cliente que la canción empezó a sonar.
    function registrarEscucha(id) {
        if (!urlHistorial) return;
        fetch(urlHistorial, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token() },
            body: 'id=' + encodeURIComponent(id)
        }).catch(function () { });
    }

    function reproducirPos(i, guardarEnPila) {
        if (!autenticado) { window.location.href = urlLogin; return; }
        if (i < 0 || i >= secuencia.length) return;
        if (guardarEnPila && idx >= 0) pila.push(idx);
        idx = i;
        audio.src = secuencia[i].audio || '';
        var pistaId = secuencia[i].id;
        audio.play().then(function () { registrarEscucha(pistaId); }).catch(function () { });
        if (localStorage.getItem('mu-vista') === 'completa') root.classList.add('is-full');
        pintar();
        if (!comPanel.hidden) cargarComentarios();
    }

    function siguiente() {
        if (idx + 1 < secuencia.length) reproducirPos(idx + 1, true);   // Queue (FIFO)
        else { audio.pause(); pintar(); }
    }

    function anterior() {
        if (audio.currentTime > 3) { audio.currentTime = 0; return; }
        var previo = pila.pop();                                        // Stack (LIFO)
        if (previo !== undefined) reproducirPos(previo, false);
        else if (idx > 0) reproducirPos(idx - 1, false);
    }

    function alternar() {
        if (!autenticado) { window.location.href = urlLogin; return; }
        if (idx < 0) { reproducirPos(0, false); return; }
        if (audio.paused) audio.play().catch(function () { }); else audio.pause();
    }

    function megusta() {
        if (!autenticado) { window.location.href = urlLogin; return; }
        var t = actual();
        if (!t) return;
        fetch(urlMeGusta, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token() },
            body: 'id=' + encodeURIComponent(t.id)
        })
            .then(function (r) { if (!r.ok) throw new Error('error'); return r.json(); })
            .then(function (d) {
                secuencia.forEach(function (x) { if (x.id === t.id) x.meGusta = d.meGusta; });
                pintar();
            })
            .catch(function () { });
    }

    // ---------- Comentarios de la canción que suena ----------
    function itemComentario(c) {
        var div = document.createElement('div');
        div.className = 'mp-com-item';
        var cab = document.createElement('div');
        cab.className = 'mp-com-user';
        var autor = document.createElement('span');
        autor.textContent = c.usuario;
        var fecha = document.createElement('span');
        fecha.textContent = new Date(c.fecha).toLocaleString('es', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
        cab.appendChild(autor);
        cab.appendChild(fecha);
        var txt = document.createElement('div');
        txt.className = 'mp-com-texto';
        txt.textContent = c.texto;
        div.appendChild(cab);
        div.appendChild(txt);
        return div;
    }

    function cargarComentarios() {
        var t = actual();
        if (!t) return;
        comLista.innerHTML = '<p class="mp-com-vacio">Cargando…</p>';
        fetch(urlComentarios + '?musicaId=' + t.id, { credentials: 'same-origin' })
            .then(function (r) { if (!r.ok) throw new Error('error'); return r.json(); })
            .then(function (lista) {
                comLista.innerHTML = '';
                if (!lista.length) {
                    comLista.innerHTML = '<p class="mp-com-vacio">Sé el primero en comentar.</p>';
                    return;
                }
                lista.forEach(function (c) { comLista.appendChild(itemComentario(c)); });
            })
            .catch(function () {
                comLista.innerHTML = '<p class="mp-com-vacio">No se pudieron cargar los comentarios.</p>';
            });
    }

    comBtn.addEventListener('click', function () {
        if (!autenticado) { window.location.href = urlLogin; return; }
        comPanel.hidden = !comPanel.hidden;
        if (!comPanel.hidden) cargarComentarios();
    });
    comCerrar.addEventListener('click', function () { comPanel.hidden = true; });

    comForm.addEventListener('submit', function (e) {
        e.preventDefault();
        var t = actual();
        var txt = comTexto.value.trim();
        if (!t || !txt) return;
        fetch(urlComentar, {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'RequestVerificationToken': token() },
            body: 'musicaId=' + t.id + '&texto=' + encodeURIComponent(txt)
        })
            .then(function (r) { if (!r.ok) throw new Error('error'); return r.json(); })
            .then(function (c) {
                var vacio = comLista.querySelector('.mp-com-vacio');
                if (vacio) vacio.remove();
                comLista.insertBefore(itemComentario(c), comLista.firstChild);
                comTexto.value = '';
            })
            .catch(function () { });
    });

    // Click en una portada: reproduce esa canción en la cola de la página.
    document.addEventListener('click', function (e) {
        var b = e.target.closest('[data-play]');
        if (!b) return;
        e.preventDefault();
        reproducirPos(Number(b.dataset.play), idx >= 0);
    });

    // Vista miniatura / completa (se recuerda en este navegador)
    function aplicarModo(modo) {
        localStorage.setItem('mu-vista', modo);
        document.querySelectorAll('.mp-modo-btn').forEach(function (b) {
            b.classList.toggle('active', b.dataset.modo === modo);
        });
    }
    document.querySelectorAll('.mp-modo-btn').forEach(function (b) {
        b.addEventListener('click', function () { aplicarModo(b.dataset.modo); });
    });
    aplicarModo(localStorage.getItem('mu-vista') === 'completa' ? 'completa' : 'miniatura');

    miniBody.addEventListener('click', function () { root.classList.add('is-full'); });
    cerrar.addEventListener('click', function () { root.classList.remove('is-full'); });

    [miniToggle, fullToggle].forEach(function (b) { b.addEventListener('click', alternar); });
    [miniPrev, fullPrev].forEach(function (b) { b.addEventListener('click', anterior); });
    [miniNext, fullNext].forEach(function (b) { b.addEventListener('click', siguiente); });
    [miniLike, fullLike].forEach(function (b) { b.addEventListener('click', megusta); });

    audio.addEventListener('play', pintar);
    audio.addEventListener('pause', pintar);
    audio.addEventListener('ended', siguiente);                          // siguiente automática
    audio.addEventListener('loadedmetadata', function () { durEl.textContent = mmss(audio.duration); });
    audio.addEventListener('timeupdate', function () {
        var pct = audio.duration ? (audio.currentTime / audio.duration) * 100 : 0;
        miniFill.style.width = pct + '%';
        seek.value = pct;
        curEl.textContent = mmss(audio.currentTime);
    });
    seek.addEventListener('input', function () {
        if (audio.duration) audio.currentTime = (seek.value / 100) * audio.duration;
    });
})();
