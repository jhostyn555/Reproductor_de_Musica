// Formulario de álbum (admin).
// - Cola (Queue, FIFO): el orden en que se marcan las canciones es el orden del tracklist.
// - Pila (Stack, LIFO): cada cambio se apila y "Deshacer" desapila el último.
(function () {
    var form = document.getElementById('albumForm');
    if (!form) return;

    var selectArtista = form.querySelector('select[name="ArtistaId"]');
    var catalogoEl = document.getElementById('al-catalogo');
    var ordenEl = document.getElementById('al-orden');
    var contadorEl = document.getElementById('al-contador');
    var tipoEl = document.getElementById('al-tipo');
    var duracionEl = document.getElementById('al-duracion');
    var sinDurEl = document.getElementById('al-sin-dur');
    var undoBtn = document.getElementById('al-undo');
    var tokenEl = form.querySelector('input[name="__RequestVerificationToken"]');
    var urlCanciones = form.dataset.artistasUrl;
    var urlDuracion = form.dataset.duracionUrl;

    var cola = [];          // Queue (FIFO) de ids de canciones
    var pila = [];          // Stack (LIFO) de cambios para deshacer
    var catalogo = {};      // id -> { id, nombre, duracionSegundos, audioUrl }
    var artistaActual = selectArtista.value;
    var pendientes = [];    // canciones a las que falta calcular la duración
    var midiendo = false;

    function tipoDe(n) {
        if (n === 0) return '—';
        if (n <= 3) return 'Single';
        if (n <= 6) return 'EP';
        return 'LP';
    }

    function mmss(s) {
        if (!s) return '--:--';
        var m = Math.floor(s / 60), r = s % 60;
        return m + ':' + (r < 10 ? '0' : '') + r;
    }

    function render() {
        // Inputs ocultos en el orden de la cola (así llegan al servidor en orden)
        form.querySelectorAll('input[name="CancionesIds"]').forEach(function (e) { e.remove(); });
        cola.forEach(function (id) {
            var h = document.createElement('input');
            h.type = 'hidden';
            h.name = 'CancionesIds';
            h.value = id;
            form.appendChild(h);
        });

        catalogoEl.querySelectorAll('input[type="checkbox"]').forEach(function (cb) {
            cb.checked = cola.indexOf(Number(cb.value)) >= 0;
        });

        ordenEl.innerHTML = '';
        cola.forEach(function (id) {
            var c = catalogo[id];
            var li = document.createElement('li');
            li.textContent = c ? c.nombre : ('Canción #' + id);
            ordenEl.appendChild(li);
        });

        var segundos = 0, sinDur = 0;
        cola.forEach(function (id) {
            var c = catalogo[id];
            if (c && c.duracionSegundos) segundos += c.duracionSegundos;
            else sinDur++;
        });

        contadorEl.textContent = cola.length;
        tipoEl.textContent = tipoDe(cola.length);
        duracionEl.textContent = Math.floor(segundos / 60) + ' min ' + (segundos % 60) + ' s';
        sinDurEl.textContent = sinDur ? (sinDur + ' canción(es) sin duración: se calculan automáticamente.') : '';
        undoBtn.disabled = pila.length === 0;
    }

    function alCambiar(id, marcada) {
        if (marcada) {
            if (cola.indexOf(id) < 0) {
                cola.push(id);                                   // encolar
                pila.push({ accion: 'agregar', id: id });        // apilar el cambio
            }
        } else {
            var pos = cola.indexOf(id);
            if (pos >= 0) {
                cola.splice(pos, 1);
                pila.push({ accion: 'quitar', id: id, pos: pos });
            }
        }
        render();
    }

    undoBtn.addEventListener('click', function () {
        var op = pila.pop();                                     // desapilar
        if (!op) return;
        if (op.accion === 'agregar') {
            var i = cola.indexOf(op.id);
            if (i >= 0) cola.splice(i, 1);
        } else if (cola.indexOf(op.id) < 0) {
            cola.splice(op.pos, 0, op.id);                       // volver a su posición
        }
        render();
    });

    // Calcula la duración de canciones que aún no la tienen (lo hace el navegador y lo guarda).
    function encolarMedicion(c) {
        pendientes.push(c);
        procesarMediciones();
    }

    function procesarMediciones() {
        if (midiendo || pendientes.length === 0) return;
        midiendo = true;
        var c = pendientes.shift();
        var audio = new Audio();
        audio.preload = 'metadata';

        function terminar() {
            midiendo = false;
            procesarMediciones();
        }

        audio.addEventListener('loadedmetadata', function () {
            var seg = Math.round(audio.duration);
            if (isFinite(seg) && seg > 0) {
                c.duracionSegundos = seg;
                var el = catalogoEl.querySelector('[data-dur="' + c.id + '"]');
                if (el) el.textContent = mmss(seg);
                var datos = new FormData();
                datos.append('id', c.id);
                datos.append('segundos', seg);
                datos.append('__RequestVerificationToken', tokenEl ? tokenEl.value : '');
                fetch(urlDuracion, { method: 'POST', body: datos, credentials: 'same-origin' }).catch(function () {});
                render();
            }
            terminar();
        });
        audio.addEventListener('error', terminar);
        audio.src = c.audioUrl;
    }

    function cargarCatalogo(artistaId, conservarSeleccion) {
        if (!conservarSeleccion) {
            cola = [];
            pila = [];
        }
        catalogo = {};
        pendientes = [];
        catalogoEl.innerHTML = '';

        if (!artistaId) {
            catalogoEl.innerHTML = '<p class="al-hint">Selecciona un artista para ver sus canciones.</p>';
            render();
            return;
        }

        catalogoEl.innerHTML = '<p class="al-hint">Cargando canciones…</p>';
        fetch(urlCanciones + '?artistaId=' + encodeURIComponent(artistaId), { credentials: 'same-origin' })
            .then(function (r) { return r.json(); })
            .then(function (lista) {
                catalogoEl.innerHTML = '';
                if (!lista.length) {
                    catalogoEl.innerHTML = '<p class="al-hint">Este artista todavía no tiene canciones.</p>';
                }
                lista.forEach(function (c) {
                    catalogo[c.id] = c;

                    var label = document.createElement('label');
                    label.className = 'al-song';
                    var cb = document.createElement('input');
                    cb.type = 'checkbox';
                    cb.value = c.id;
                    cb.addEventListener('change', function () { alCambiar(c.id, cb.checked); });

                    var nombre = document.createElement('span');
                    nombre.textContent = c.nombre;
                    var dur = document.createElement('small');
                    dur.dataset.dur = c.id;
                    dur.textContent = mmss(c.duracionSegundos);

                    label.appendChild(cb);
                    label.appendChild(nombre);
                    label.appendChild(dur);
                    catalogoEl.appendChild(label);

                    if (!c.duracionSegundos && c.audioUrl) encolarMedicion(c);
                });
                render();
            })
            .catch(function () {
                catalogoEl.innerHTML = '<p class="al-hint">No se pudieron cargar las canciones.</p>';
            });
    }

    selectArtista.addEventListener('change', function () {
        if (cola.length && !confirm('Cambiar de artista vaciará la lista de canciones. ¿Continuar?')) {
            selectArtista.value = artistaActual;
            return;
        }
        artistaActual = selectArtista.value;
        cargarCatalogo(artistaActual, false);
    });

    // Al editar, se conserva la selección guardada (en el orden original)
    var inicial = [];
    try { inicial = JSON.parse(form.dataset.seleccionadas || '[]').map(Number); } catch (e) { inicial = []; }
    cola = inicial.slice();
    cargarCatalogo(artistaActual, true);
})();
