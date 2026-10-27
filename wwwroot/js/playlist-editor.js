// Editor de playlists (admin y cliente).
// - Cola (Queue, FIFO): el orden de la lista es el orden de reproducción. Lo que se agrega va al final.
// - Pila (Stack, LIFO): cada agregar o quitar se apila; "Deshacer" desapila el último cambio.
// - Buscador: filtra las canciones disponibles por nombre, artista o género.
(function () {
    var root = document.getElementById('pl-editor');
    if (!root) return;

    var disp = document.getElementById('pl-disponibles');
    var colaEl = document.getElementById('pl-cola');
    var ocultos = document.getElementById('pl-hidden');
    var undoBtn = document.getElementById('pl-undo');
    var contador = document.getElementById('pl-contador');
    var buscar = document.getElementById('pl-buscar');

    var cola = [];   // Queue: ids en orden de reproducción
    var pila = [];   // Stack: operaciones para deshacer

    function info(id) {
        var el = disp.querySelector('[data-id="' + id + '"]');
        return el ? { nombre: el.dataset.nombre, artista: el.dataset.artista, genero: el.dataset.genero } : null;
    }

    function render() {
        ocultos.innerHTML = '';
        cola.forEach(function (id) {
            var h = document.createElement('input');
            h.type = 'hidden';
            h.name = 'MusicaIds';
            h.value = id;
            ocultos.appendChild(h);
        });

        colaEl.innerHTML = '';
        cola.forEach(function (id, i) {
            var c = info(id) || { nombre: 'Canción #' + id, artista: '', genero: '' };
            var li = document.createElement('li');
            li.dataset.id = id;
            var num = document.createElement('span');
            num.className = 'pl-num';
            num.textContent = (i + 1) + '.';
            var nom = document.createElement('span');
            nom.textContent = c.nombre + (c.artista ? ' — ' + c.artista : '');
            var quitar = document.createElement('button');
            quitar.type = 'button';
            quitar.className = 'pl-quitar';
            quitar.dataset.remove = id;
            quitar.title = 'Quitar';
            quitar.textContent = '✕';
            li.appendChild(num);
            li.appendChild(nom);
            li.appendChild(quitar);
            colaEl.appendChild(li);
        });

        disp.querySelectorAll('[data-id]').forEach(function (el) {
            el.classList.toggle('is-added', cola.indexOf(Number(el.dataset.id)) >= 0);
        });
        contador.textContent = cola.length;
        undoBtn.disabled = pila.length === 0;
    }

    function agregar(id) {
        if (cola.indexOf(id) >= 0) return;
        cola.push(id);                              // Enqueue (FIFO)
        pila.push({ op: 'agregar', id: id });       // Push
        render();
    }

    function quitar(id) {
        var pos = cola.indexOf(id);
        if (pos < 0) return;
        cola.splice(pos, 1);
        pila.push({ op: 'quitar', id: id, pos: pos });
        render();
    }

    function deshacer() {
        var op = pila.pop();                        // Pop (LIFO)
        if (!op) return;
        if (op.op === 'agregar') {
            var i = cola.indexOf(op.id);
            if (i >= 0) cola.splice(i, 1);
        } else if (cola.indexOf(op.id) < 0) {
            cola.splice(op.pos, 0, op.id);          // vuelve a su posición original
        }
        render();
    }

    disp.addEventListener('click', function (e) {
        var b = e.target.closest('[data-id]');
        if (b) agregar(Number(b.dataset.id));
    });

    colaEl.addEventListener('click', function (e) {
        var b = e.target.closest('[data-remove]');
        if (b) quitar(Number(b.dataset.remove));
    });

    undoBtn.addEventListener('click', deshacer);

    buscar.addEventListener('input', function () {
        var t = buscar.value.toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
        disp.querySelectorAll('[data-id]').forEach(function (el) {
            var txt = (el.dataset.nombre + ' ' + el.dataset.artista + ' ' + el.dataset.genero)
                .toLowerCase().normalize('NFD').replace(/[\u0300-\u036f]/g, '');
            el.hidden = t.length > 0 && txt.indexOf(t) < 0;
        });
    });

    root.addEventListener('submit', function (e) {
        if (cola.length === 0) {
            e.preventDefault();
            alert('Agrega al menos una canción a la playlist.');
        }
    });

    // Estado inicial: la cola que llegó del servidor (en orden).
    colaEl.querySelectorAll('li').forEach(function (li) { cola.push(Number(li.dataset.id)); });
    render();
})();
