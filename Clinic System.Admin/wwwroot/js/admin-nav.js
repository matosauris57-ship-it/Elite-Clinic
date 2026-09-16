window.dentalCareNav = {
    shellSelector: '.admin-shell',
    openClass: 'nav-open',

    shell() {
        return document.querySelector(this.shellSelector);
    },

    isOpen() {
        const el = this.shell();
        return !!el && el.classList.contains(this.openClass);
    },

    setOpen(open) {
        const el = this.shell();
        if (!el) return;
        el.classList.toggle(this.openClass, !!open);
        const toggle = el.querySelector('[data-nav-toggle]');
        if (toggle) {
            toggle.setAttribute('aria-expanded', open ? 'true' : 'false');
            toggle.setAttribute('aria-label', open ? 'Cerrar menú' : 'Abrir menú');
        }
    },

    toggle() {
        this.setOpen(!this.isOpen());
    },

    close() {
        this.setOpen(false);
    }
};

document.addEventListener('click', function (e) {
    if (e.target.closest('.sidebar .nav-item')) {
        window.dentalCareNav.close();
    }
});

document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        window.dentalCareNav.close();
    }
});

document.addEventListener('blazor:enhancedload', function () {
    window.dentalCareNav.close();
});
