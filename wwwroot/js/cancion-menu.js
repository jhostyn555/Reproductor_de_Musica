// Menú de tres puntos de cada canción: abre/cierra el menú y cierra al hacer click fuera o con Escape.
(function () {
    // Se abre hacia arriba y a la derecha; si no cabe, se abre hacia abajo o se corre para no quedar tapado.
    function posicionar(pop) {
        pop.classList.remove('mm-down');
        pop.style.transform = '';
        var r = pop.getBoundingClientRect();
        if (r.top < 70) pop.classList.add('mm-down');

        // Borde izquierdo disponible: el ancho del menú lateral (en móvil está fuera de pantalla, vale 0)
        var side = document.querySelector('.up-sidebar');
        var minLeft = Math.max(8, side ? side.getBoundingClientRect().right + 8 : 8);
        if (r.left < minLeft) pop.style.transform = 'translateX(' + (minLeft - r.left) + 'px)';
    }

    function cerrarTodos() {
        document.querySelectorAll('[data-mm-pop]').forEach(function (pop) {
            pop.hidden = true;
            pop.classList.remove('mm-down');
            pop.style.transform = '';
            var card = pop.closest('.mu-card');
            if (card) card.classList.remove('is-open');
            var toggle = pop.parentElement.querySelector('[data-mm-toggle]');
            if (toggle) toggle.setAttribute('aria-expanded', 'false');
        });
    }

    document.addEventListener('click', function (e) {
        var toggle = e.target.closest('[data-mm-toggle]');
        if (toggle) {
            e.preventDefault();
            var pop = toggle.parentElement.querySelector('[data-mm-pop]');
            var abrir = pop.hidden;
            cerrarTodos();
            if (abrir) {
                pop.hidden = false;
                posicionar(pop);
                toggle.setAttribute('aria-expanded', 'true');
                var card = pop.closest('.mu-card');
                if (card) card.classList.add('is-open');
            }
            return;
        }
        if (!e.target.closest('[data-mm-pop]')) cerrarTodos();
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') cerrarTodos();
    });
})();
