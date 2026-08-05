// Toast helper built on Bootstrap 5 Toasts.
const TOAST_ICONS = { success: '✓', error: '✕', warning: '⚠', info: 'ℹ', danger: '✕' };
const TOAST_BG = { success: 'bg-success', error: 'bg-danger', warning: 'bg-warning text-dark', danger: 'bg-danger', info: 'bg-info text-dark' };

export function showToast(message, type = 'info', title = '') {
    const container = document.getElementById('toast-container');
    if (!container) return;
    const id = 'toast-' + Date.now() + '-' + Math.floor(Math.random() * 1000);
    const icon = TOAST_ICONS[type] || TOAST_ICONS.info;
    const bg = TOAST_BG[type] || TOAST_BG.info;
    const html = `
        <div id="${id}" class="toast align-items-center text-white ${bg} border-0" role="alert" aria-live="assertive" aria-atomic="true" data-bs-delay="4000">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2">
                    <span style="font-weight:700">${icon}</span>
                    <div>${title ? '<strong>' + title + '</strong>' : ''}<div class="small">${message}</div></div>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;
    container.insertAdjacentHTML('beforeend', html);
    const el = document.getElementById(id);
    const t = new bootstrap.Toast(el, { delay: type === 'error' ? 6000 : 4000 });
    t.show();
    el.addEventListener('hidden.bs.toast', () => el.remove());
}

// Pull server-rendered flash messages from hidden inputs and show as toasts.
export function flushFlashMessages() {
    document.querySelectorAll('input[data-toast]').forEach(el => {
        showToast(el.getAttribute('data-toast-msg') || '', el.getAttribute('data-toast'), el.getAttribute('data-toast-title') || '');
        el.remove();
    });
}

window.showToast = showToast;
