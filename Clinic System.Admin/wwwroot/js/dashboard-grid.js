window.dentalCareDashboardGrid = (function () {
    const instances = {};
    var rowHeight = 80;

    function columnsFor(width) {
        if (width < 640) return 1;
        return 12;
    }

    function metrics(el) {
        var cols = columnsFor(el.clientWidth || window.innerWidth);
        var gap = 12;
        var col = Math.max(1, (el.clientWidth - gap * (cols - 1)) / cols);
        return { cols: cols, gap: gap, col: col, row: rowHeight + gap };
    }

    function clamp(value, min, max) {
        return Math.max(min, Math.min(max, value));
    }

    function readInt(value, fallback) {
        var n = parseInt(value, 10);
        return Number.isFinite(n) ? n : fallback;
    }

    function readPlacement(cell) {
        return {
            x: readInt(cell.getAttribute('data-x') || cell.style.getPropertyValue('--x'), 0),
            y: readInt(cell.getAttribute('data-y') || cell.style.getPropertyValue('--y'), 0),
            w: readInt(cell.getAttribute('data-w') || cell.style.getPropertyValue('--w'), 3),
            h: readInt(cell.getAttribute('data-h') || cell.style.getPropertyValue('--h'), 2)
        };
    }

    function applyPlacement(cell, x, y, w, h) {
        cell.setAttribute('data-x', String(x));
        cell.setAttribute('data-y', String(y));
        cell.setAttribute('data-w', String(w));
        cell.setAttribute('data-h', String(h));
        cell.style.setProperty('--x', String(x));
        cell.style.setProperty('--y', String(y));
        cell.style.setProperty('--w', String(w));
        cell.style.setProperty('--h', String(h));
        cell.style.gridColumn = (x + 1) + ' / span ' + w;
        cell.style.gridRow = (y + 1) + ' / span ' + h;
        cell.style.transform = '';
    }

    function isDragSource(target) {
        if (!target || !target.closest) return false;
        if (target.closest('.widget-tool-btn') || target.closest('.widget-resize-handle'))
            return false;
        return !!(target.closest('.widget-drag-handle') || target.closest('.widget-toolbar'));
    }

    function notify(inst, id, x, y, w, h) {
        if (!inst.dotNet || !id) return;
        inst.dotNet.invokeMethodAsync('OnCellMoved', id, x, y, w, h);
    }

    function bind(inst) {
        var opts = { capture: true, passive: false };
        inst.el.addEventListener('pointerdown', inst.onPointerDown, opts);
        window.addEventListener('pointermove', inst.onPointerMove, opts);
        window.addEventListener('pointerup', inst.onPointerUp, opts);
        window.addEventListener('pointercancel', inst.onPointerUp, opts);
    }

    function unbind(inst) {
        var opts = { capture: true };
        inst.el.removeEventListener('pointerdown', inst.onPointerDown, opts);
        window.removeEventListener('pointermove', inst.onPointerMove, opts);
        window.removeEventListener('pointerup', inst.onPointerUp, opts);
        window.removeEventListener('pointercancel', inst.onPointerUp, opts);
    }

    function init(elementId, editMode, dotNetRef) {
        var el = document.getElementById(elementId);
        if (!el) return false;
        destroy(elementId);

        var inst = { el: el, editMode: !!editMode, dotNet: dotNetRef, drag: null, resize: null };
        instances[elementId] = inst;

        inst.onPointerDown = function (event) {
            if (!inst.editMode) return;
            if (event.button != null && event.button !== 0) return;
            if (!inst.el.contains(event.target)) return;

            var resize = event.target.closest('.widget-resize-handle');
            var dragging = !resize && isDragSource(event.target);
            if (!resize && !dragging) return;

            var cell = event.target.closest('.dash-cell');
            if (!cell || !inst.el.contains(cell)) return;

            event.preventDefault();
            event.stopPropagation();

            var place = readPlacement(cell);
            var start = {
                id: cell.getAttribute('data-widget-id'),
                cell: cell,
                x: place.x,
                y: place.y,
                w: place.w,
                h: place.h,
                minW: readInt(cell.getAttribute('data-min-w'), 2),
                minH: readInt(cell.getAttribute('data-min-h'), 2),
                maxW: readInt(cell.getAttribute('data-max-w'), 12),
                maxH: readInt(cell.getAttribute('data-max-h'), 8),
                pointerX: event.clientX,
                pointerY: event.clientY,
                pointerId: event.pointerId
            };

            if (resize) {
                inst.resize = start;
                cell.classList.add('is-resizing');
            } else {
                inst.drag = start;
                cell.classList.add('is-dragging');
            }

            try { cell.setPointerCapture(event.pointerId); } catch (e) { }
        };

        inst.onPointerMove = function (event) {
            var action = inst.drag || inst.resize;
            if (!action) return;
            if (action.pointerId != null && event.pointerId !== action.pointerId) return;
            event.preventDefault();
            var cell = action.cell;
            if (inst.drag) {
                cell.style.transform = 'translate(' + (event.clientX - action.pointerX) + 'px,' + (event.clientY - action.pointerY) + 'px)';
            } else if (inst.resize) {
                var m = metrics(inst.el);
                var dw = Math.round((event.clientX - action.pointerX) / (m.col + m.gap));
                var dh = Math.round((event.clientY - action.pointerY) / m.row);
                var nextW = clamp(action.w + dw, action.minW, Math.min(action.maxW, m.cols - action.x));
                var nextH = clamp(action.h + dh, action.minH, action.maxH);
                applyPlacement(cell, action.x, action.y, nextW, nextH);
            }
        };

        inst.onPointerUp = function (event) {
            var action = inst.drag || inst.resize;
            if (!action) return;
            if (action.pointerId != null && event.pointerId !== action.pointerId) return;
            var cell = action.cell;
            cell.classList.remove('is-dragging', 'is-resizing');
            try { cell.releasePointerCapture(action.pointerId); } catch (e) { }

            var m = metrics(inst.el);
            if (m.cols === 1) {
                applyPlacement(cell, action.x, action.y, action.w, action.h);
            } else if (inst.drag) {
                var next = {
                    x: clamp(action.x + Math.round((event.clientX - action.pointerX) / (m.col + m.gap)), 0, Math.max(0, m.cols - action.w)),
                    y: Math.max(0, action.y + Math.round((event.clientY - action.pointerY) / m.row)),
                    w: action.w,
                    h: action.h
                };
                applyPlacement(cell, next.x, next.y, next.w, next.h);
                notify(inst, action.id, next.x, next.y, next.w, next.h);
            } else if (inst.resize) {
                var dw = Math.round((event.clientX - action.pointerX) / (m.col + m.gap));
                var dh = Math.round((event.clientY - action.pointerY) / m.row);
                var nextW = clamp(action.w + dw, action.minW, Math.min(action.maxW, m.cols - action.x));
                var nextH = clamp(action.h + dh, action.minH, action.maxH);
                applyPlacement(cell, action.x, action.y, nextW, nextH);
                notify(inst, action.id, action.x, action.y, nextW, nextH);
            }

            inst.drag = null;
            inst.resize = null;
        };

        bind(inst);
        return true;
    }

    function setEditMode(elementId, editMode) {
        var inst = instances[elementId];
        var el = document.getElementById(elementId);
        if (!inst || !el || inst.el !== el)
            return init(elementId, editMode, inst ? inst.dotNet : null);
        inst.editMode = !!editMode;
        return true;
    }

    function destroy(elementId) {
        var inst = instances[elementId];
        if (!inst) return;
        unbind(inst);
        delete instances[elementId];
    }

    return { init: init, setEditMode: setEditMode, destroy: destroy };
})();
