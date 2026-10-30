window.devGames = {
    // Returns the document-order indexes of elements in the HTML fragment matched by the selector, or null if invalid.
    selectorMatches: function (html, selector) {
        const tpl = document.createElement('template');
        tpl.innerHTML = html;
        const root = tpl.content;
        const all = Array.from(root.querySelectorAll('*'));
        try {
            const hits = new Set(root.querySelectorAll(selector));
            return all.map((el, i) => (hits.has(el) ? i : -1)).filter(i => i >= 0);
        } catch {
            return null;
        }
    }
};
