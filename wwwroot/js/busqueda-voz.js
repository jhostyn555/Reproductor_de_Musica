// Búsqueda por voz (Web Speech API) para el panel del cliente.
// - "ir a perfil", "abre historial", "perfil", "cerrar sesión"  → navega a la opción del menú.
// - Cualquier otra frase ("Bad Bunny", "busca cumbia")          → busca en Músicas, igual que el buscador.
// Las opciones se leen del propio menú lateral (.up-menu-item), así que siguen al menú si cambia.
(function () {
    var SR = window.SpeechRecognition || window.webkitSpeechRecognition;
    if (!SR) return;   // el navegador no tiene reconocimiento de voz: no se muestra el botón

    function norm(s) {
        return (s || '').toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '')
            .replace(/[.,!?¿¡;:]/g, ' ').replace(/\s+/g, ' ').trim();
    }

    var ARTICULOS = ['el', 'la', 'los', 'las', 'al', 'a', 'de', 'del', 'en', 'por'];
    function sinArticulos(s) {
        return s.split(' ').filter(function (w) { return w && ARTICULOS.indexOf(w) < 0; }).join(' ');
    }

    // Formas alternativas para cada opción del menú (clave = etiqueta normalizada).
    var SINONIMOS = {
        'inicio': ['inicio', 'casa', 'home', 'principal'],
        'musicas': ['musicas', 'musica', 'canciones'],
        'historial': ['historial', 'escuchados', 'historia'],
        'novedades': ['novedades', 'novedad', 'nuevos'],
        'perfil': ['perfil', 'mi perfil', 'cuenta'],
        'panel admin': ['panel admin', 'admin', 'administrador', 'panel de admin'],
        'cerrar sesion': ['cerrar sesion', 'salir', 'cerrar', 'logout']
    };

    var VERBOS_IR = ['quiero ir a', 'quiero ir al', 'ir a', 'ir al', 've a', 've al', 'ir', 'abre', 'abrir',
                     'llevame a', 'llevame al', 'llevame', 'entra a', 'entrar a', 'muestra', 'mostrar', 'ver'];
    var VERBOS_BUSCAR = ['quiero escuchar', 'quiero', 'buscame', 'buscar', 'busca', 'busco',
                         'reproducir', 'reproduce', 'escuchar', 'pon', 'poner'];

    function opcionesMenu() {
        var lista = [];
        document.querySelectorAll('.up-menu-item').forEach(function (li) {
            var etq = li.querySelector('.up-menu-label');
            var etiqueta = norm(etq ? etq.textContent : li.textContent);
            var clic = li.querySelector('a, button');
            if (etiqueta && clic) lista.push({ etiqueta: etiqueta, elemento: clic, href: clic.getAttribute('href') || '' });
        });
        return lista;
    }

    function buscarOpcion(texto) {
        var t = sinArticulos(norm(texto));
        if (!t) return null;
        var ops = opcionesMenu();
        for (var i = 0; i < ops.length; i++) {
            var alias = SINONIMOS[ops[i].etiqueta] || [ops[i].etiqueta];
            for (var j = 0; j < alias.length; j++) {
                if (sinArticulos(norm(alias[j])) === t) return ops[i];
            }
        }
        return null;
    }

    // Si la frase empieza con el prefijo, devuelve lo que sigue; si no, null.
    function quitarPrefijo(t, prefijo) {
        if (t === prefijo) return '';
        if (t.indexOf(prefijo + ' ') === 0) return t.slice(prefijo.length + 1);
        return null;
    }

    function interpretar(textoOriginal) {
        var t = norm(textoOriginal);

        // 1) "ir a perfil", "abre historial", "llévame a novedades"
        for (var i = 0; i < VERBOS_IR.length; i++) {
            var resto = quitarPrefijo(t, VERBOS_IR[i]);
            if (resto) {
                var op = buscarOpcion(resto);
                if (op) return { tipo: 'menu', op: op };
            }
        }

        // 2) Solo el nombre de la opción: "perfil", "musicas", "cerrar sesión"
        var directo = buscarOpcion(t);
        if (directo) return { tipo: 'menu', op: directo };

        // 3) Búsqueda: quitar verbos de búsqueda del inicio
        var q = t;
        for (var k = 0; k < VERBOS_BUSCAR.length; k++) {
            var r = quitarPrefijo(q, VERBOS_BUSCAR[k]);
            if (r !== null) { q = r; break; }
        }
        q = q.replace(/^(la |el |las |los )?(cancion|canciones|musica|artista)( de)? /, '').trim();
        return { tipo: 'buscar', q: q };
    }

    function ejecutar(res) {
        if (res.tipo === 'menu') {
            if (res.op.etiqueta === 'cerrar sesion') {
                if (confirm('¿Cerrar sesión?')) res.op.elemento.click();
                return 'Cerrando sesión';
            }
            if (res.op.elemento.tagName === 'A' && res.op.href) location.href = res.op.href;
            else res.op.elemento.click();
            return 'Abriendo ' + res.op.etiqueta;
        }

        // Búsqueda: misma página y mismo filtro que el buscador de Músicas
        var musicas = opcionesMenu().filter(function (o) { return o.etiqueta === 'musicas'; })[0];
        var base = musicas && musicas.href ? musicas.href.split('?')[0] : '/User/Musicas';
        location.href = base + '?vista=genero' + (res.q ? '&q=' + encodeURIComponent(res.q) : '');
        return res.q ? 'Buscando: ' + res.q : 'Abriendo Músicas';
    }

    // ---------- Botón flotante y mensajes ----------
    var css = document.createElement('style');
    css.textContent =
        '#voz-btn{position:fixed;right:16px;bottom:96px;z-index:9998;width:56px;height:56px;border-radius:50%;border:none;cursor:pointer;color:#04121a;background:linear-gradient(135deg,#22d3ee,#a78bfa);box-shadow:0 10px 26px rgba(0,0,0,.4);display:flex;align-items:center;justify-content:center}' +
        '#voz-btn.escuchando{background:#f87171;color:#fff;animation:voz-pulso 1.2s infinite}' +
        '@keyframes voz-pulso{0%{box-shadow:0 0 0 0 rgba(248,113,113,.6)}100%{box-shadow:0 0 0 18px rgba(248,113,113,0)}}' +
        '#voz-estado{position:fixed;right:16px;bottom:162px;z-index:9998;max-width:270px;padding:10px 14px;border-radius:12px;background:#111827;color:#e5e7eb;font:500 13px system-ui,sans-serif;box-shadow:0 8px 24px rgba(0,0,0,.4)}' +
        '#voz-estado[hidden]{display:none}';
    document.head.appendChild(css);

    var btn = document.createElement('button');
    btn.id = 'voz-btn';
    btn.type = 'button';
    btn.title = 'Búsqueda por voz';
    btn.setAttribute('aria-label', 'Búsqueda por voz');
    btn.innerHTML = '<svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="9" y="2" width="6" height="12" rx="3"/><path d="M5 10a7 7 0 0 0 14 0"/><line x1="12" y1="19" x2="12" y2="22"/></svg>';

    var estado = document.createElement('div');
    estado.id = 'voz-estado';
    estado.hidden = true;

    document.body.appendChild(btn);
    document.body.appendChild(estado);

    var ocultarTimer = null;
    function mostrar(texto) {
        estado.textContent = texto;
        estado.hidden = false;
        clearTimeout(ocultarTimer);
    }
    function ocultarLuego() {
        clearTimeout(ocultarTimer);
        ocultarTimer = setTimeout(function () { estado.hidden = true; }, 4000);
    }

    var reco = new SR();
    reco.lang = 'es-ES';
    reco.interimResults = false;
    reco.maxAlternatives = 1;
    reco.continuous = false;

    var escuchando = false;
    function terminar() {
        escuchando = false;
        btn.classList.remove('escuchando');
        ocultarLuego();
    }

    reco.onresult = function (e) {
        var texto = e.results[0][0].transcript;
        mostrar('“' + texto + '”');
        mostrar(ejecutar(interpretar(texto)));
    };
    reco.onerror = function (e) {
        if (e.error === 'not-allowed') mostrar('Permite el uso del micrófono en el navegador');
        else if (e.error === 'no-speech') mostrar('No te escuché, intenta de nuevo');
        else mostrar('Error de voz: ' + e.error);
        terminar();
    };
    reco.onend = terminar;

    btn.addEventListener('click', function () {
        if (escuchando) { reco.stop(); return; }
        try {
            escuchando = true;
            btn.classList.add('escuchando');
            mostrar('Escuchando… di "ir a perfil" o el nombre de una canción');
            reco.start();
        } catch (err) {
            terminar();
        }
    });
})();
