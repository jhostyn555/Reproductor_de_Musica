// Menú de tres puntos de cada canción: abre/cierra el menú y cierra al hacer click fuera o con Escape.
(function () {
    function cerrarTodos() {
        document.querySelectorAll('[data-mm-pop]').forEach(function (pop) {
            pop.hidden = true;
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
