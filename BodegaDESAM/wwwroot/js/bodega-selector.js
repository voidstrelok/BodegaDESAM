window.bodegaSelector = {
    save: function (id) {
        try { localStorage.setItem('bodega-activa', String(id)); } catch { }
        document.cookie = 'bodega-activa=' + encodeURIComponent(id) + '; path=/; max-age=31536000; samesite=lax';
    }
};
