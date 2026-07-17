import { showToast, flushFlashMessages } from './toast.js';

// ---- Loading state on form submit (prevent double-submit + spinner) ----
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

// ---- Mobile rail toggle ----
function initMobileNav() {
    const toggle = document.getElementById('railToggle');
    const rail = document.getElementById('appRail');
    const backdrop = document.getElementById('railBackdrop');
    if (!toggle || !rail) return;
    const close = () => { rail.classList.remove('open'); backdrop && backdrop.classList.remove('show'); document.body.classList.remove('rail-open'); toggle.setAttribute('aria-expanded', 'false'); };
    const open = () => { rail.classList.add('open'); backdrop && backdrop.classList.add('show'); document.body.classList.add('rail-open'); toggle.setAttribute('aria-expanded', 'true'); };
    toggle.addEventListener('click', () => {
        if (rail.classList.contains('open')) close(); else open();
    });
    backdrop && backdrop.addEventListener('click', close);
    document.addEventListener('keydown', e => { if (e.key === 'Escape') close(); });
}

// ---- AJAX filter forms: serialize and fetch HTML fragment ----
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

document.addEventListener('DOMContentLoaded', () => {
    attachSubmitGuards();
    initMobileNav();
    initAjaxForms();
    flushFlashMessages();
});

window.addEventListener('pageshow', attachSubmitGuards);
