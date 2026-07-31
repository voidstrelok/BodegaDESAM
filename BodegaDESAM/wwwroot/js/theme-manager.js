// Theme Manager for BodegaDESAM
// Feature de tema oscuro desactivada temporalmente — siempre tema claro
(function () {
    const THEME_KEY = 'bodega-theme';

    // Limpiar cualquier preferencia oscura guardada y forzar tema claro
    try { localStorage.removeItem(THEME_KEY); } catch (e) { /* ignore */ }
    document.documentElement.removeAttribute('data-theme');
})();
