window.dentalCareWaitingRoom = (function () {
    function toggleFullscreen() {
        var el = document.documentElement;
        if (!document.fullscreenElement) {
            if (el.requestFullscreen) return el.requestFullscreen();
            if (el.webkitRequestFullscreen) return el.webkitRequestFullscreen();
        } else {
            if (document.exitFullscreen) return document.exitFullscreen();
            if (document.webkitExitFullscreen) return document.webkitExitFullscreen();
        }
        return Promise.resolve();
    }

    function speak(text) {
        if (!text || !window.speechSynthesis) return;
        try {
            window.speechSynthesis.cancel();
            var utter = new SpeechSynthesisUtterance(String(text));
            utter.lang = 'es-ES';
            utter.rate = 0.95;
            window.speechSynthesis.speak(utter);
        } catch (_) {
            /* ignore */
        }
    }

    return {
        toggleFullscreen: toggleFullscreen,
        speak: speak
    };
})();
