window.dentalCareLoader = {
    play(el) {
        if (!el) return;
        el.muted = true;
        el.defaultMuted = true;
        el.playsInline = true;
        const play = el.play();
        if (play && typeof play.catch === 'function') {
            play.catch(function () { });
        }
    }
};
