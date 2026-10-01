window.devTools = {
    registerShortcuts: (dotnetRef) => {
        document.addEventListener('keydown', e => {
            if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
                e.preventDefault();
                dotnetRef.invokeMethodAsync('OpenPalette');
            }
        });
    },
    focus: (el) => { if (el) el.focus(); },
    downloadBase64: (fileName, mime, base64) => {
        const a = document.createElement('a');
        a.href = `data:${mime};base64,${base64}`;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();
    },
    downloadText: (fileName, mime, text) => {
        const blob = new Blob([text], { type: mime });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(url);
    },
    userAgent: () => navigator.userAgent
};
