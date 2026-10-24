// Formularios de canciones: detecta la duración del audio (archivo o enlace) y la guarda en el campo oculto.
(function () {
    var archivo = document.getElementById('ArchivoAudio');
    var enlace = document.getElementById('UrlAudio');
    var oculto = document.getElementById('DuracionSegundos');
    if (!oculto) return;

    var info = document.createElement('small');
    info.style.display = 'block';
    info.style.color = 'var(--txt-muted, #888)';
    info.style.marginTop = '4px';
    oculto.parentNode.insertBefore(info, oculto.nextSibling);

    function medir(src) {
        if (!src) return;
        var audio = new Audio();
        audio.preload = 'metadata';
        audio.addEventListener('loadedmetadata', function () {
            var seg = Math.round(audio.duration);
            if (isFinite(seg) && seg > 0) {
                oculto.value = seg;
                info.textContent = 'Duración detectada: ' + Math.floor(seg / 60) + ':' + String(seg % 60).padStart(2, '0');
            }
        });
        audio.src = src;
    }

    if (archivo) {
        archivo.addEventListener('change', function () {
            var f = archivo.files && archivo.files[0];
            if (f) medir(URL.createObjectURL(f));
        });
    }
    if (enlace) {
        enlace.addEventListener('change', function () { medir(enlace.value.trim()); });
    }
})();
