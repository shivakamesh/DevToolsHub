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
    printHtml: (html) => {
        const frame = document.createElement('iframe');
        frame.setAttribute('sandbox', 'allow-modals allow-same-origin');
        frame.style.cssText = 'position:fixed;right:0;bottom:0;width:0;height:0;border:0;visibility:hidden';
        frame.onload = () => {
            const w = frame.contentWindow;
            w.focus();
            w.print();
            setTimeout(() => frame.remove(), 1000);
        };
        frame.srcdoc = html;
        document.body.appendChild(frame);
    },
    userAgent: () => navigator.userAgent
};
