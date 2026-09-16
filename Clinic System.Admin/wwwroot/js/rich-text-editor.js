window.dentalCareRichText = {
    instances: {},

    init(id, element, dotNetRef, html, disabled) {
        this.dispose(id);
        if (!element) return;

        element.innerHTML = this.toEditorHtml(html);
        element.contentEditable = disabled ? 'false' : 'true';

        const onInput = () => {
            dotNetRef.invokeMethodAsync('OnHtmlChanged', element.innerHTML);
        };
        const onFocus = () => {
            dotNetRef.invokeMethodAsync('OnEditorFocus');
        };
        const onPaste = (event) => {
            event.preventDefault();
            const text = (event.clipboardData || window.clipboardData).getData('text/plain') || '';
            document.execCommand('insertText', false, text);
        };

        element.addEventListener('input', onInput);
        element.addEventListener('focus', onFocus);
        element.addEventListener('paste', onPaste);

        this.instances[id] = { element, dotNetRef, onInput, onFocus, onPaste };
    },

    dispose(id) {
        const inst = this.instances[id];
        if (!inst) return;
        inst.element.removeEventListener('input', inst.onInput);
        inst.element.removeEventListener('focus', inst.onFocus);
        inst.element.removeEventListener('paste', inst.onPaste);
        delete this.instances[id];
    },

    setHtml(id, html) {
        const inst = this.instances[id];
        if (!inst) return;
        if (document.activeElement === inst.element) return;
        const next = this.toEditorHtml(html);
        if (inst.element.innerHTML === next) return;
        inst.element.innerHTML = next;
    },

    setDisabled(id, disabled) {
        const inst = this.instances[id];
        if (!inst) return;
        inst.element.contentEditable = disabled ? 'false' : 'true';
    },

    exec(id, command, value) {
        const inst = this.instances[id];
        if (!inst || inst.element.contentEditable === 'false') return;
        inst.element.focus();
        if (command === 'createLink') {
            const url = window.prompt('Dirección del enlace', 'https://');
            if (!url) return;
            document.execCommand('createLink', false, url);
        } else {
            document.execCommand(command, false, value ?? null);
        }
        inst.dotNetRef.invokeMethodAsync('OnHtmlChanged', inst.element.innerHTML);
    },

    insertText(id, text) {
        const inst = this.instances[id];
        if (!inst || inst.element.contentEditable === 'false') return;
        inst.element.focus();
        document.execCommand('insertText', false, text);
        inst.dotNetRef.invokeMethodAsync('OnHtmlChanged', inst.element.innerHTML);
    },

    toEditorHtml(value) {
        if (!value) return '';
        if (/<\/?[a-zA-Z][^>]*>/.test(value)) return value;
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/\r\n/g, '\n')
            .replace(/\n/g, '<br>');
    }
};
