import { showToast, flushFlashMessages } from './toast.js';

function initThemeToggle() {
    const btn = document.getElementById('themeToggle');
    if (!btn) return;
    btn.addEventListener('click', () => {
        const cur = document.documentElement.getAttribute('data-theme') === 'dark' ? 'dark' : 'light';
        const next = cur === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-theme', next);
        try { localStorage.setItem('theme', next); } catch (e) {}
    });
}

function attachSubmitGuards() {
    document.querySelectorAll('form').forEach(form => {
        if (form.__guarded) return;
        form.__guarded = true;
        form.addEventListener('submit', (e) => {
            if (form.checkValidity && !form.checkValidity()) return;
            form.querySelectorAll('button:not([type="button"]):not([data-no-busy]), input[type="submit"]').forEach(b => {
                b.disabled = true;
                if (!b.dataset.__label) b.dataset.__label = b.innerHTML;
                if (!b.querySelector('.spinner-border') && b.tagName === 'BUTTON') {
                    b.insertAdjacentHTML('afterbegin', '<span class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>');
                }
            });
        });
    });
}

function initMobileNav() {
    const toggle = document.getElementById('railToggle');
    const rail = document.getElementById('appRail');
    const backdrop = document.getElementById('railBackdrop');
    if (!toggle || !rail) return;
    const close = () => { rail.classList.remove('open'); backdrop && backdrop.classList.remove('show'); document.body.classList.remove('rail-open'); toggle.setAttribute('aria-expanded', 'false'); };
    const open = () => { rail.classList.add('open'); backdrop && backdrop.classList.add('show'); document.body.classList.add('rail-open'); toggle.setAttribute('aria-expanded', 'true'); };
    toggle.addEventListener('click', () => { if (rail.classList.contains('open')) close(); else open(); });
    backdrop && backdrop.addEventListener('click', close);
    document.addEventListener('keydown', e => { if (e.key === 'Escape') close(); });
}

function initAjaxForms() {
    document.querySelectorAll('form[data-ajax="true"]').forEach(form => {
        const target = form.getAttribute('data-ajax-target');
        const onSuccess = (html) => {
            const parser = new DOMParser().parseFromString(html, 'text/html');
            const frag = parser.querySelector(target);
            const here = document.querySelector(target);
            if (frag && here) {
                here.outerHTML = frag.outerHTML;
                const url = new URL(form.action, location.href);
                const fd = new FormData(form);
                fd.forEach((v, k) => { if (v) url.searchParams.set(k, v); });
                history.replaceState(null, '', url);
                attachSubmitGuards();
                initTableSort();
            }
        };
        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            const fd = new FormData(form);
            const params = new URLSearchParams();
            fd.forEach((v, k) => { if (v !== '' && v != null) params.set(k, v); });
            const resp = await fetch(form.action + (params.toString() ? '?' + params : ''), {
                headers: { 'X-Requested-With': 'XMLHttpRequest' }
            });
            if (resp.ok) { onSuccess(await resp.text()); showToast('Updated', 'info'); }
            else { showToast('Filter failed', 'error'); }
        });
    });
}

function getCell(row, key) {
    const cell = row.querySelector('[data-key="' + key + '"]');
    return cell ? cell.textContent.trim() : (row.cells[parseInt(key)] ? row.cells[parseInt(key)].textContent.trim() : '');
}
function initTableSort() {
    document.querySelectorAll('table[data-sortable]').forEach(table => {
        const headers = table.querySelectorAll('thead th[data-sort]');
        headers.forEach(th => {
            th.style.cursor = 'pointer';
            th.title = 'Sort by ' + (th.dataset.sortLabel || th.textContent.trim());
            th.addEventListener('click', () => {
                const key = th.dataset.sort;
                const dir = th.dataset.dir === 'asc' ? 'desc' : 'asc';
                table.querySelectorAll('thead th').forEach(h => { h.dataset.dir = ''; h.classList.remove('sorted-asc', 'sorted-desc'); });
                th.dataset.dir = dir;
                th.classList.add(dir === 'asc' ? 'sorted-asc' : 'sorted-desc');
                const tbody = table.querySelector('tbody');
                const rows = Array.from(tbody.querySelectorAll('tr'));
                rows.sort((a, b) => {
                    const av = getCell(a, key), bv = getCell(b, key);
                    const an = parseFloat(av), bn = parseFloat(bv);
                    if (!isNaN(an) && !isNaN(bn) && av !== '' && bv !== '') return dir === 'asc' ? an - bn : bn - an;
                    return dir === 'asc' ? av.localeCompare(bv) : bv.localeCompare(av);
                });
                rows.forEach(r => tbody.appendChild(r));
            });
        });
    });
}

function initShortcuts() {
    const help = document.getElementById('shortcutHelp');
    document.addEventListener('keydown', (e) => {
        const t = e.target;
        if (t && (t.tagName === 'INPUT' || t.tagName === 'TEXTAREA' || t.tagName === 'SELECT' || t.isContentEditable)) return;
        if (e.key === '?') { e.preventDefault(); if (help) new bootstrap.Modal(help).show(); return; }
        if (e.key === '/') { e.preventDefault(); const s = document.querySelector('.topbar-search input'); if (s) s.focus(); return; }
        if (e.key === 'g' || e.key === 'G') { window.__keySeq = 'g'; setTimeout(() => { if (window.__keySeq === 'g') window.__keySeq = ''; }, 800); return; }
        if (window.__keySeq === 'g' && (e.key === 'e' || e.key === 'E')) {
            window.__keySeq = ''; e.preventDefault();
            const a = document.querySelector('a.nav-link[href*="/Exceptions"]');
            if (a) location.href = a.getAttribute('href');
        }
    });
}

function initMarkdownEditors() {
    document.querySelectorAll('[data-md-editor]').forEach(wrap => {
        const ta = wrap.querySelector('textarea');
        const toolbar = wrap.querySelector('.md-toolbar');
        if (!ta || !toolbar) return;
        toolbar.querySelectorAll('button[data-md]').forEach(btn => {
            btn.addEventListener('click', () => {
                const kind = btn.dataset.md;
                const start = ta.selectionStart, end = ta.selectionEnd;
                const hasSel = start !== end;
                const sel = hasSel ? ta.value.substring(start, end) : ta.value;
                const s0 = hasSel ? start : 0;
                const e0 = hasSel ? end : ta.value.length;
                let insert = sel, caret = s0;
                const wrapWith = (pre, post) => { insert = pre + (sel || '') + post; caret = s0 + pre.length + (hasSel ? sel.length : ta.value.length); };
                const linePrefix = (pfx) => { insert = pfx + (sel || 'text'); caret = s0 + pfx.length + (hasSel ? sel.length : ta.value.length); };
                if (kind === 'bold') wrapWith('**', '**');
                else if (kind === 'italic') wrapWith('*', '*');
                else if (kind === 'code') wrapWith('`', '`');
                else if (kind === 'list') linePrefix('- ');
                else if (kind === 'link') { insert = '[' + (sel || 'text') + '](url)'; caret = s0 + 1 + (sel ? sel.length : 4); }
                ta.value = ta.value.substring(0, s0) + insert + ta.value.substring(e0);
                ta.focus();
                ta.selectionStart = caret; ta.selectionEnd = caret;
            });
        });
    });
}

document.addEventListener('DOMContentLoaded', () => {
    initThemeToggle();
    attachSubmitGuards();
    initMobileNav();
    initAjaxForms();
    initTableSort();
    initShortcuts();
    initMarkdownEditors();
    flushFlashMessages();
});

window.addEventListener('pageshow', () => { attachSubmitGuards(); initTableSort(); });
