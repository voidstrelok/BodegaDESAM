window.bodega = window.bodega || {};

window.bodega.getSelectedValues = (selectEl) => {
    if (!selectEl || !selectEl.options) return [];

    const result = [];
    for (const opt of selectEl.options) {
        if (opt.selected) result.push(opt.value);
    }

    return result;
};
