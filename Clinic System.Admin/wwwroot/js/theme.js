window.dentalCareTheme = {
    storageKey: 'dc-theme',
    cookieKey: 'dc-theme',
    _persistenceReady: false,

    getSaved() {
        const saved = localStorage.getItem(this.storageKey);
        return saved === 'light' || saved === 'dark' ? saved : null;
    },

    getSystem() {
        return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
    },

    resolve() {
        return this.getSaved() ?? this.getSystem();
    },

    apply(theme) {
        document.documentElement.setAttribute('data-theme', theme);
    },

    syncCookie(theme) {
        document.cookie = `${this.cookieKey}=${theme};path=/;max-age=31536000;SameSite=Lax`;
    },

    set(theme) {
        if (theme !== 'light' && theme !== 'dark') return;
        localStorage.setItem(this.storageKey, theme);
        this.syncCookie(theme);
        this.apply(theme);
    },

    toggle() {
        const next = this.resolve() === 'dark' ? 'light' : 'dark';
        this.set(next);
        return next;
    },

    getCurrent() {
        return this.resolve();
    },

    init() {
        const theme = this.resolve();
        this.syncCookie(theme);
        this.apply(theme);
    },

    initPersistence() {
        this.init();

        if (this._persistenceReady) return;
        this._persistenceReady = true;

        const reapply = () => this.apply(this.resolve());

        if (typeof Blazor !== 'undefined') {
            Blazor.addEventListener('enhancedload', reapply);
        }
    }
};

window.dentalCareUi = {
    scrollToBookingError() {
        document.getElementById('booking-error')?.scrollIntoView({
            behavior: 'smooth',
            block: 'center'
        });
    },

    insertAtCursor(element, text) {
        if (!element) return null;

        const value = element.value ?? '';
        const start = Number.isInteger(element.selectionStart) ? element.selectionStart : value.length;
        const end = Number.isInteger(element.selectionEnd) ? element.selectionEnd : start;
        const next = value.slice(0, start) + text + value.slice(end);
        const cursor = start + text.length;

        element.value = next;
        element.focus();
        element.setSelectionRange(cursor, cursor);
        element.dispatchEvent(new Event('input', { bubbles: true }));
        element.scrollIntoView({ behavior: 'smooth', block: 'center' });
        return next;
    },

    printPage() {
        window.print();
    },

    focusById(id) {
        const el = document.getElementById(id);
        if (!el) return;
        el.focus();
        if (typeof el.select === 'function') el.select();
    }
};
