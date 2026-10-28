// Helpers for media, PDF, diagram and minifier tools. Third-party libraries are loaded on demand.
window.devExtra = (() => {
    const CDN = {
        htmlToImage: 'https://cdn.jsdelivr.net/npm/html-to-image@1.11.11/dist/html-to-image.js',
        hljs: 'https://cdn.jsdelivr.net/gh/highlightjs/cdn-release@11.9.0/build/highlight.min.js',
        pdfLib: 'https://cdn.jsdelivr.net/npm/pdf-lib@1.17.1/dist/pdf-lib.min.js',
        jszip: 'https://cdn.jsdelivr.net/npm/jszip@3.10.1/dist/jszip.min.js',
        mermaid: 'https://cdn.jsdelivr.net/npm/mermaid@10.9.1/dist/mermaid.min.js',
        sourceMap: 'https://cdn.jsdelivr.net/npm/source-map@0.7.4/dist/source-map.js',
        terser: 'https://cdn.jsdelivr.net/npm/terser@5.31.0/dist/bundle.min.js',
        beautifyJs: 'https://cdn.jsdelivr.net/npm/js-beautify@1.15.1/js/lib/beautify.min.js',
        beautifyCss: 'https://cdn.jsdelivr.net/npm/js-beautify@1.15.1/js/lib/beautify-css.min.js',
        beautifyHtml: 'https://cdn.jsdelivr.net/npm/js-beautify@1.15.1/js/lib/beautify-html.min.js'
    };

    const loaded = {};
    const loadScript = (url) => loaded[url] ??= new Promise((resolve, reject) => {
        const s = document.createElement('script');
        s.src = url;
        s.async = true;
        s.onload = resolve;
        s.onerror = () => { delete loaded[url]; reject(new Error('Could not load ' + url)); };
        document.head.appendChild(s);
    });
    const load = async (...keys) => { for (const k of keys) await loadScript(CDN[k]); };

    const files = new Map();
    let nextId = 1;

    const blobUrl = (blob) => URL.createObjectURL(blob);

    const download = (url, fileName) => {
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        a.remove();
    };

    const loadImage = (src) => new Promise((resolve, reject) => {
        const img = new Image();
        img.onload = () => resolve(img);
        img.onerror = () => reject(new Error('The file could not be decoded as an image.'));
        img.src = src;
    });

    const canvasToBlob = (canvas, type, quality) =>
        new Promise(resolve => canvas.toBlob(b => resolve(b), type, quality));

    const fetchBlob = async (url) => (await fetch(url)).blob();

    return {
        // ---------- files ----------
        pickFiles: (input) => {
            const result = [];
            for (const f of input.files) {
                const id = String(nextId++);
                files.set(id, f);
                result.push({ id, name: f.name, type: f.type, size: f.size, url: URL.createObjectURL(f) });
            }
            input.value = '';
            return result;
        },
        releaseFile: (id) => files.delete(id),
        download,
        revoke: (url) => { if (url) URL.revokeObjectURL(url); },

        downloadZip: async (entries, zipName) => {
            await load('jszip');
            const zip = new JSZip();
            for (const e of entries) {
                if (e.text !== undefined && e.text !== null) zip.file(e.name, e.text);
                else zip.file(e.name, await fetchBlob(e.url));
            }
            const blob = await zip.generateAsync({ type: 'blob' });
            const url = blobUrl(blob);
            download(url, zipName);
            setTimeout(() => URL.revokeObjectURL(url), 5000);
        },

        // ---------- image compressor ----------
        compressImage: async (id, format, quality, maxWidth, maxHeight) => {
            const file = files.get(id);
            if (!file) throw new Error('File not found.');
            const src = URL.createObjectURL(file);
            try {
                const img = await loadImage(src);
                let w = img.naturalWidth, h = img.naturalHeight;
                const scale = Math.min(1, maxWidth > 0 ? maxWidth / w : 1, maxHeight > 0 ? maxHeight / h : 1);
                w = Math.max(1, Math.round(w * scale));
                h = Math.max(1, Math.round(h * scale));
                const canvas = document.createElement('canvas');
                canvas.width = w;
                canvas.height = h;
                const ctx = canvas.getContext('2d');
                if (format === 'image/jpeg') { ctx.fillStyle = '#fff'; ctx.fillRect(0, 0, w, h); }
                ctx.imageSmoothingQuality = 'high';
                ctx.drawImage(img, 0, 0, w, h);
                const blob = await canvasToBlob(canvas, format, quality);
                if (!blob) throw new Error('This browser cannot encode ' + format + '.');
                return { url: blobUrl(blob), size: blob.size, width: w, height: h, originalWidth: img.naturalWidth, originalHeight: img.naturalHeight };
            } finally {
                URL.revokeObjectURL(src);
            }
        },

        // ---------- code to image ----------
        highlight: async (el, code, language) => {
            await load('hljs');
            el.textContent = code;
            el.className = 'hljs';
            if (language && language !== 'auto' && hljs.getLanguage(language)) {
                el.innerHTML = hljs.highlight(code, { language, ignoreIllegals: true }).value;
            } else {
                el.innerHTML = hljs.highlightAuto(code).value;
            }
        },
        exportPng: async (el, fileName, scale) => {
            await load('htmlToImage');
            const url = await htmlToImage.toPng(el, { pixelRatio: scale || 2, cacheBust: true });
            download(url, fileName);
        },
        copyPng: async (el, scale) => {
            await load('htmlToImage');
            const blob = await htmlToImage.toBlob(el, { pixelRatio: scale || 2, cacheBust: true });
            await navigator.clipboard.write([new ClipboardItem({ 'image/png': blob })]);
        },

        // ---------- PDF tools ----------
        pdfPageCount: async (id) => {
            await load('pdfLib');
            const doc = await PDFLib.PDFDocument.load(await files.get(id).arrayBuffer(), { ignoreEncryption: true });
            return doc.getPageCount();
        },
        pdfMerge: async (ids) => {
            await load('pdfLib');
            const out = await PDFLib.PDFDocument.create();
            for (const id of ids) {
                const src = await PDFLib.PDFDocument.load(await files.get(id).arrayBuffer(), { ignoreEncryption: true });
                const pages = await out.copyPages(src, src.getPageIndices());
                pages.forEach(p => out.addPage(p));
            }
            const bytes = await out.save({ useObjectStreams: true });
            return { url: blobUrl(new Blob([bytes], { type: 'application/pdf' })), size: bytes.length };
        },
        pdfExtract: async (id, groups) => {
            await load('pdfLib');
            const src = await PDFLib.PDFDocument.load(await files.get(id).arrayBuffer(), { ignoreEncryption: true });
            const result = [];
            for (const pages of groups) {
                const out = await PDFLib.PDFDocument.create();
                const copied = await out.copyPages(src, pages);
                copied.forEach(p => out.addPage(p));
                const bytes = await out.save({ useObjectStreams: true });
                result.push({ url: blobUrl(new Blob([bytes], { type: 'application/pdf' })), size: bytes.length });
            }
            return result;
        },
        pdfOptimize: async (id, stripMetadata) => {
            await load('pdfLib');
            const doc = await PDFLib.PDFDocument.load(await files.get(id).arrayBuffer(), { ignoreEncryption: true, updateMetadata: false });
            if (stripMetadata) {
                doc.setTitle(''); doc.setAuthor(''); doc.setSubject(''); doc.setKeywords([]);
                doc.setCreator(''); doc.setProducer('');
            }
            const bytes = await doc.save({ useObjectStreams: true });
            return { url: blobUrl(new Blob([bytes], { type: 'application/pdf' })), size: bytes.length };
        },
        imagesToPdf: async (ids, pageSize, margin) => {
            await load('pdfLib');
            const sizes = { a4: [595.28, 841.89], letter: [612, 792] };
            const out = await PDFLib.PDFDocument.create();
            for (const id of ids) {
                const file = files.get(id);
                const src = URL.createObjectURL(file);
                let bytes, isPng = file.type === 'image/png';
                try {
                    if (file.type === 'image/jpeg' || isPng) {
                        bytes = await file.arrayBuffer();
                    } else {
                        const img = await loadImage(src);
                        const c = document.createElement('canvas');
                        c.width = img.naturalWidth; c.height = img.naturalHeight;
                        c.getContext('2d').drawImage(img, 0, 0);
                        bytes = await (await canvasToBlob(c, 'image/png')).arrayBuffer();
                        isPng = true;
                    }
                } finally { URL.revokeObjectURL(src); }
                const image = isPng ? await out.embedPng(bytes) : await out.embedJpg(bytes);
                const [pw, ph] = pageSize === 'fit'
                    ? [image.width + margin * 2, image.height + margin * 2]
                    : (image.width > image.height ? [...sizes[pageSize]].reverse() : sizes[pageSize]);
                const page = out.addPage([pw, ph]);
                const s = Math.min((pw - margin * 2) / image.width, (ph - margin * 2) / image.height, pageSize === 'fit' ? 1 : Infinity);
                const w = image.width * s, h = image.height * s;
                page.drawImage(image, { x: (pw - w) / 2, y: (ph - h) / 2, width: w, height: h });
            }
            const bytes = await out.save({ useObjectStreams: true });
            return { url: blobUrl(new Blob([bytes], { type: 'application/pdf' })), size: bytes.length };
        },

        // ---------- Mermaid ----------
        renderMermaid: async (el, code, theme) => {
            await load('mermaid');
            mermaid.initialize({ startOnLoad: false, theme, securityLevel: 'strict', htmlLabels: false, flowchart: { htmlLabels: false } });
            try {
                const { svg } = await mermaid.render('mmd' + Date.now(), code);
                el.innerHTML = svg;
                return null;
            } catch (e) {
                document.querySelectorAll('[id^="dmmd"]').forEach(n => n.remove());
                return (e && e.message) || String(e);
            }
        },
        exportSvg: (el, fileName) => {
            const svg = el.querySelector('svg');
            if (!svg) return;
            const url = blobUrl(new Blob([new XMLSerializer().serializeToString(svg)], { type: 'image/svg+xml' }));
            download(url, fileName);
            setTimeout(() => URL.revokeObjectURL(url), 5000);
        },
        exportSvgPng: async (el, fileName, scale, background) => {
            const svg = el.querySelector('svg');
            if (!svg) return;
            const box = svg.getBoundingClientRect();
            const clone = svg.cloneNode(true);
            clone.setAttribute('width', box.width);
            clone.setAttribute('height', box.height);
            const data = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(new XMLSerializer().serializeToString(clone));
            const img = await loadImage(data);
            const c = document.createElement('canvas');
            c.width = Math.ceil(box.width * scale);
            c.height = Math.ceil(box.height * scale);
            const ctx = c.getContext('2d');
            if (background) { ctx.fillStyle = background; ctx.fillRect(0, 0, c.width, c.height); }
            ctx.scale(scale, scale);
            ctx.drawImage(img, 0, 0, box.width, box.height);
            const url = blobUrl(await canvasToBlob(c, 'image/png'));
            download(url, fileName);
            setTimeout(() => URL.revokeObjectURL(url), 5000);
        },

        // ---------- minifier / beautifier ----------
        minifyJs: async (code) => {
            await load('sourceMap', 'terser');
            const r = await Terser.minify(code, { compress: true, mangle: true });
            return r.code ?? '';
        },
        beautify: async (language, code, indent) => {
            await load('beautifyJs', 'beautifyCss', 'beautifyHtml');
            const opts = { indent_size: indent, end_with_newline: true, preserve_newlines: true, max_preserve_newlines: 2 };
            if (language === 'css') return css_beautify(code, opts);
            if (language === 'html') return html_beautify(code, opts);
            return js_beautify(code, opts);
        },

        // ---------- favicon ----------
        renderFavicons: async (sourceId, text, background, foreground, radius, padding, sizes) => {
            let img = null;
            if (sourceId) {
                const src = URL.createObjectURL(files.get(sourceId));
                try { img = await loadImage(src); } finally { URL.revokeObjectURL(src); }
            }
            const out = [];
            for (const size of sizes) {
                const c = document.createElement('canvas');
                c.width = c.height = size;
                const ctx = c.getContext('2d');
                const r = size * radius / 100;
                if (background) {
                    ctx.fillStyle = background;
                    ctx.beginPath();
                    ctx.roundRect(0, 0, size, size, r);
                    ctx.fill();
                }
                const pad = size * padding / 100;
                const inner = size - pad * 2;
                if (img) {
                    const s = Math.min(inner / img.naturalWidth, inner / img.naturalHeight);
                    const w = img.naturalWidth * s, h = img.naturalHeight * s;
                    ctx.imageSmoothingQuality = 'high';
                    ctx.drawImage(img, (size - w) / 2, (size - h) / 2, w, h);
                } else if (text) {
                    ctx.fillStyle = foreground;
                    ctx.textAlign = 'center';
                    ctx.textBaseline = 'middle';
                    let fontSize = inner;
                    ctx.font = `700 ${fontSize}px Inter, system-ui, sans-serif`;
                    const m = ctx.measureText(text).width;
                    if (m > inner) { fontSize = fontSize * inner / m; ctx.font = `700 ${fontSize}px Inter, system-ui, sans-serif`; }
                    ctx.fillText(text, size / 2, size / 2 + fontSize * 0.06);
                }
                const blob = await canvasToBlob(c, 'image/png');
                out.push({ size, url: blobUrl(blob) });
            }
            return out;
        },
        buildIco: async (pngUrls, sizes) => {
            const images = [];
            for (const u of pngUrls) images.push(new Uint8Array(await (await fetchBlob(u)).arrayBuffer()));
            const headerSize = 6 + 16 * images.length;
            const total = headerSize + images.reduce((n, i) => n + i.length, 0);
            const buf = new Uint8Array(total);
            const dv = new DataView(buf.buffer);
            dv.setUint16(0, 0, true); dv.setUint16(2, 1, true); dv.setUint16(4, images.length, true);
            let offset = headerSize;
            images.forEach((data, i) => {
                const e = 6 + i * 16, s = sizes[i];
                buf[e] = s >= 256 ? 0 : s; buf[e + 1] = s >= 256 ? 0 : s;
                dv.setUint16(e + 4, 1, true); dv.setUint16(e + 6, 32, true);
                dv.setUint32(e + 8, data.length, true); dv.setUint32(e + 12, offset, true);
                buf.set(data, offset);
                offset += data.length;
            });
            return blobUrl(new Blob([buf], { type: 'image/x-icon' }));
        }
    };
})();
