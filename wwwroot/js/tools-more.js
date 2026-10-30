// Helpers for the SQL mystery game (sql.js loaded on demand) and AES/RSA tools (WebCrypto).
window.devMore = (() => {
    const SQLJS = 'https://cdn.jsdelivr.net/npm/sql.js@1.10.3/dist/';
    let sqlPromise = null;
    let db = null;

    function loadScript(src) {
        return new Promise((resolve, reject) => {
            const s = document.createElement('script');
            s.src = src;
            s.onload = resolve;
            s.onerror = () => reject(new Error('Failed to load ' + src));
            document.head.appendChild(s);
        });
    }

    async function sqlInit(schema) {
        if (!sqlPromise) {
            sqlPromise = loadScript(SQLJS + 'sql-wasm.js').then(() => window.initSqlJs({ locateFile: f => SQLJS + f }));
        }
        const SQL = await sqlPromise;
        if (db) db.close();
        db = new SQL.Database();
        db.run(schema);
        return true;
    }

    function sqlExec(query) {
        if (!db) return { error: 'Database not loaded yet.', columns: [], rows: [] };
        try {
            const res = db.exec(query);
            if (res.length === 0) return { error: null, columns: [], rows: [] };
            const last = res[res.length - 1];
            return {
                error: null,
                columns: last.columns,
                rows: last.values.slice(0, 200).map(r => r.map(v => v === null ? 'NULL' : String(v)))
            };
        } catch (e) {
            return { error: e.message, columns: [], rows: [] };
        }
    }

    const enc = new TextEncoder();
    const dec = new TextDecoder();
    const toB64 = buf => btoa(String.fromCharCode(...new Uint8Array(buf)));
    const fromB64 = s => Uint8Array.from(atob(s.replace(/\s+/g, '')), c => c.charCodeAt(0));

    async function aesKey(password, salt, iterations) {
        const base = await crypto.subtle.importKey('raw', enc.encode(password), 'PBKDF2', false, ['deriveKey']);
        return crypto.subtle.deriveKey({ name: 'PBKDF2', salt, iterations, hash: 'SHA-256' },
            base, { name: 'AES-GCM', length: 256 }, false, ['encrypt', 'decrypt']);
    }

    // Output: Base64(salt[16] | iv[12] | ciphertext+tag)
    async function aesEncrypt(password, text, iterations) {
        const salt = crypto.getRandomValues(new Uint8Array(16));
        const iv = crypto.getRandomValues(new Uint8Array(12));
        const key = await aesKey(password, salt, iterations);
        const ct = new Uint8Array(await crypto.subtle.encrypt({ name: 'AES-GCM', iv }, key, enc.encode(text)));
        const out = new Uint8Array(salt.length + iv.length + ct.length);
        out.set(salt, 0); out.set(iv, 16); out.set(ct, 28);
        return toB64(out);
    }

    async function aesDecrypt(password, b64, iterations) {
        const data = fromB64(b64);
        if (data.length < 29) throw new Error('Ciphertext is too short.');
        const key = await aesKey(password, data.slice(0, 16), iterations);
        try {
            const pt = await crypto.subtle.decrypt({ name: 'AES-GCM', iv: data.slice(16, 28) }, key, data.slice(28));
            return dec.decode(pt);
        } catch {
            throw new Error('Decryption failed: wrong password or corrupted data.');
        }
    }

    const pem = (label, buf) => `-----BEGIN ${label}-----\n${toB64(buf).match(/.{1,64}/g).join('\n')}\n-----END ${label}-----`;
    const unpem = text => fromB64(text.replace(/-----[^-]+-----/g, ''));
    const RSA = { name: 'RSA-OAEP', hash: 'SHA-256' };

    async function rsaGenerate(bits) {
        const pair = await crypto.subtle.generateKey({ ...RSA, modulusLength: bits, publicExponent: new Uint8Array([1, 0, 1]) }, true, ['encrypt', 'decrypt']);
        return {
            publicKey: pem('PUBLIC KEY', await crypto.subtle.exportKey('spki', pair.publicKey)),
            privateKey: pem('PRIVATE KEY', await crypto.subtle.exportKey('pkcs8', pair.privateKey))
        };
    }

    async function rsaEncrypt(publicPem, text) {
        const key = await crypto.subtle.importKey('spki', unpem(publicPem), RSA, false, ['encrypt']);
        return toB64(await crypto.subtle.encrypt({ name: 'RSA-OAEP' }, key, enc.encode(text)));
    }

    async function rsaDecrypt(privatePem, b64) {
        const key = await crypto.subtle.importKey('pkcs8', unpem(privatePem), RSA, false, ['decrypt']);
        try {
            return dec.decode(await crypto.subtle.decrypt({ name: 'RSA-OAEP' }, key, fromB64(b64)));
        } catch {
            throw new Error('Decryption failed: wrong key or corrupted data.');
        }
    }

    return { sqlInit, sqlExec, aesEncrypt, aesDecrypt, rsaGenerate, rsaEncrypt, rsaDecrypt };
})();
