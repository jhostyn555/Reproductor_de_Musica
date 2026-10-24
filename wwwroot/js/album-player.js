// Reproductor de álbum (admin y cliente).
// - Cola (Queue, FIFO): canciones que suenan después. Se agregan con "+" o al tocar el nombre.
// - Pila (Stack, LIFO): historial de canciones reproducidas. "Anterior" desapila.
// Mientras suena una canción, elegir otra la agrega a la cola: la actual no se detiene.
(function () {
    var root = document.getElementById('album-root');
    if (!root) return;

    var pistas = [];
    try { pistas = JSON.parse(root.dataset.tracks || '[]'); } catch (e) { pistas = []; }
    var porId = {};
    pistas.forEach(function (t) { porId[t.id] = t; });

    function $(id) { return document.getElementById(id); }

    var audio = new Audio();
    var cola = [];   // Queue (FIFO)
    var pila = [];   // Stack (LIFO)
    var actual = null;

    var bar = $('ab-bar');
    var nowImg = $('ab-now-img'), nowTitulo = $('ab-now-title'), nowArtista = $('ab-now-artist');
    var btnPrev = $('ab-prev'), btnToggle = $('ab-toggle'), btnNext = $('ab-next');
    var seek = $('ab-seek'), curEl = $('ab-cur'), durEl = $('ab-dur');
    var qnEl = $('ab-qn'), hnEl = $('ab-hn');
    var btnTodo = $('ab-play-all');
    var btnLista = $('ab-view-lista'), btnMini = $('ab-view-mini');
    var vistaLista = $('ab-lista'), vistaMini = $('ab-miniaturas');

    function mmss(s) {
        if (!isFinite(s) || s <= 0) return '0:00';
        var m = Math.floor(s / 60), r = Math.floor(s % 60);
        return m + ':' + (r < 10 ? '0' : '') + r;
    }

    function marcarItems() {
        root.querySelectorAll('.ab-item').forEach(function (el) {
            var id = Number(el.dataset.trackId);
            var esActual = !!actual && actual.id === id;
            el.classList.toggle('is-current', esActual);
            el.classList.toggle('is-playing', esActual && !audio.paused);
        });
    }

    function pintar() {
        bar.hidden = !actual;
        if (actual) {
            nowImg.src = actual.imagen || '';
            nowTitulo.textContent = actual.nombre;
            nowArtista.textContent = actual.artista;
        }
        btnToggle.textContent = audio.paused ? '▶' : '⏸';
        qnEl.textContent = cola.length;
        hnEl.textContent = pila.length;
        marcarItems();
    }

    function cambiarVista(miniaturas) {
        vistaLista.hidden = miniaturas;
        vistaMini.hidden = !miniaturas;
        root.classList.toggle('is-tiles', miniaturas);
        btnLista.classList.toggle('active', !miniaturas);
        btnMini.classList.toggle('active', miniaturas);
        btnLista.setAttribute('aria-pressed', String(!miniaturas));
        btnMini.setAttribute('aria-pressed', String(miniaturas));
    }

    // Reproduce una pista. Si había una sonando, la manda a la pila (historial).
    function reproducir(t, guardarActualEnPila) {
        if (actual && guardarActualEnPila) pila.push(actual);
        actual = t;
        audio.src = t.audio || '';
        audio.play().catch(function () { });
        pintar();
    }

    function siguiente() {
        var t = cola.shift();                       // Dequeue (FIFO)
        if (t) {
            reproducir(t, true);
        } else {
            audio.pause();
            audio.removeAttribute('src');
            actual = null;
            pintar();
        }
    }

    function anterior() {
        var t = pila.pop();                         // Pop (LIFO)
        if (!t) return;
        if (actual) cola.unshift(actual);           // la actual vuelve al frente de la cola
        reproducir(t, false);
    }

    // Agregar a la cola: si nada suena empieza a sonar; si no, espera su turno.
    function encolar(t) {
        if (!actual) { reproducir(t, false); return; }
        cola.push(t);                               // Enqueue (FIFO)
        pintar();
    }

    // Lista y miniaturas usan los mismos atributos: data-action y data-track-id.
    root.addEventListener('click', function (e) {
        var el = e.target.closest('[data-action]');
        if (!el) return;
        var item = el.closest('[data-track-id]');
        var t = item ? porId[Number(item.dataset.trackId)] : null;
        if (!t) return;

        if (el.dataset.action === 'ahora') {
            reproducir(t, true);                    // reproducir ahora (interrumpe)
        } else {
            encolar(t);                             // no interrumpe la canción actual
        }
    });

    btnTodo.addEventListener('click', function () {
        if (!pistas.length) return;
        if (!actual) {
            cola = pistas.slice(1);
            reproducir(pistas[0], false);
        } else {
            cola = cola.concat(pistas);
            pintar();
        }
    });

    btnLista.addEventListener('click', function () { cambiarVista(false); });
    btnMini.addEventListener('click', function () { cambiarVista(true); });

    btnToggle.addEventListener('click', function () {
        if (!actual) { siguiente(); return; }
        if (audio.paused) audio.play().catch(function () { }); else audio.pause();
    });
    btnNext.addEventListener('click', siguiente);
    btnPrev.addEventListener('click', anterior);

    audio.addEventListener('play', pintar);
    audio.addEventListener('pause', pintar);
    audio.addEventListener('ended', siguiente);
    audio.addEventListener('loadedmetadata', function () { durEl.textContent = mmss(audio.duration); });
    audio.addEventListener('timeupdate', function () {
        curEl.textContent = mmss(audio.currentTime);
        if (audio.duration) seek.value = (audio.currentTime / audio.duration) * 100;
    });
    seek.addEventListener('input', function () {
        if (audio.duration) audio.currentTime = (seek.value / 100) * audio.duration;
    });
})();
