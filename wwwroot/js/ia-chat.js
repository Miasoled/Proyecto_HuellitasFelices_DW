(function () {
    const form = document.getElementById('chat-form');
    const input = document.getElementById('chat-input');
    const messages = document.getElementById('chat-messages');
    const btnEnviar = document.getElementById('btn-enviar');

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        const prompt = input.value.trim();
        if (!prompt) return;

        input.value = '';
        agregarMensaje(prompt, 'user');
        mostrarCargando();

        btnEnviar.disabled = true;
        btnEnviar.innerHTML = '<i class="bi bi-hourglass-split"></i>';

        try {
            const response = await fetch('/IA/Generar', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ prompt: prompt })
            });

            const data = await response.json();
            eliminarCargando();
            agregarMensaje(data.respuesta || 'No se obtuvo respuesta.', 'bot');
        } catch (err) {
            eliminarCargando();
            agregarMensaje('Error de conexion. Verifique que el servidor este funcionando.', 'bot');
        }

        btnEnviar.disabled = false;
        btnEnviar.innerHTML = '<i class="bi bi-send"></i> Enviar';
        input.focus();
    });

    window.enviarSugerencia = function (btn) {
        input.value = btn.textContent;
        form.dispatchEvent(new Event('submit'));
    };

    function agregarMensaje(texto, tipo) {
        const esBot = tipo === 'bot';
        const msgDiv = document.createElement('div');
        msgDiv.className = 'chat-msg mb-3';

        if (esBot) {
            msgDiv.innerHTML = `
                <div class="d-flex align-items-start gap-2">
                    <div style="width:36px;height:36px;background:var(--primary);border-radius:50%;display:flex;align-items:center;justify-content:center;flex-shrink:0">
                        <i class="bi bi-robot text-white" style="font-size:.9rem"></i>
                    </div>
                    <div style="background:var(--bg-secondary);border:1px solid var(--border);border-radius:12px;padding:12px 16px;max-width:85%">
                        <p class="mb-0" style="font-size:.9rem;white-space:pre-wrap">${escapeHtml(texto)}</p>
                    </div>
                </div>`;
        } else {
            msgDiv.innerHTML = `
                <div class="d-flex align-items-start gap-2 justify-content-end">
                    <div style="background:var(--primary);color:white;border-radius:12px;padding:12px 16px;max-width:85%">
                        <p class="mb-0" style="font-size:.9rem">${escapeHtml(texto)}</p>
                    </div>
                    <div style="width:36px;height:36px;background:var(--gray-200);border-radius:50%;display:flex;align-items:center;justify-content:center;flex-shrink:0">
                        <i class="bi bi-person" style="font-size:.9rem"></i>
                    </div>
                </div>`;
        }

        messages.appendChild(msgDiv);
        messages.scrollTop = messages.scrollHeight;
    }

    function mostrarCargando() {
        const loader = document.createElement('div');
        loader.id = 'chat-loading';
        loader.className = 'chat-msg mb-3';
        loader.innerHTML = `
            <div class="d-flex align-items-start gap-2">
                <div style="width:36px;height:36px;background:var(--primary);border-radius:50%;display:flex;align-items:center;justify-content:center;flex-shrink:0">
                    <i class="bi bi-robot text-white" style="font-size:.9rem"></i>
                </div>
                <div style="background:var(--bg-secondary);border:1px solid var(--border);border-radius:12px;padding:12px 16px;">
                    <div class="d-flex align-items-center gap-2">
                        <div class="spinner-border spinner-border-sm text-primary" role="status"></div>
                        <span style="font-size:.85rem;color:var(--text-soft)">Pensando...</span>
                    </div>
                </div>
            </div>`;
        messages.appendChild(loader);
        messages.scrollTop = messages.scrollHeight;
    }

    function eliminarCargando() {
        const loader = document.getElementById('chat-loading');
        if (loader) loader.remove();
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
})();
