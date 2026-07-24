(function () {
    const bubble = document.getElementById('chatbot-bubble');
    const panel = document.getElementById('chatbot-panel');
    const closeBtn = document.getElementById('chatbot-close');
    const messages = document.getElementById('chatbot-messages');
    const input = document.getElementById('chatbot-input');
    const form = document.getElementById('chatbot-form');
    const sendBtn = document.getElementById('chatbot-send');

    let isOpen = false;

    bubble.addEventListener('click', function () {
        isOpen = !isOpen;
        if (isOpen) {
            panel.classList.add('open');
            bubble.classList.add('active');
            bubble.querySelector('i').className = 'bi bi-x-lg';
            input.focus();
        } else {
            panel.classList.remove('open');
            bubble.classList.remove('active');
            bubble.querySelector('i').className = 'bi bi-chat-dots-fill';
        }
    });

    closeBtn.addEventListener('click', function () {
        isOpen = false;
        panel.classList.remove('open');
        bubble.classList.remove('active');
        bubble.querySelector('i').className = 'bi bi-chat-dots-fill';
    });

    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        const prompt = input.value.trim();
        if (!prompt) return;

        input.value = '';
        agregarMensaje(prompt, 'user');
        ocultarSugerencias();
        mostrarCargando();
        sendBtn.disabled = true;

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
            agregarMensaje('Error de conexion con el servidor.', 'bot');
        }

        sendBtn.disabled = false;
        input.focus();
    });

    // Sugerencias
    document.querySelectorAll('.chatbot-suggestion-btn').forEach(function (btn) {
        btn.addEventListener('click', function () {
            input.value = btn.textContent;
            form.dispatchEvent(new Event('submit'));
        });
    });

    function agregarMensaje(texto, tipo) {
        const esBot = tipo === 'bot';
        const msgDiv = document.createElement('div');
        msgDiv.className = 'chatbot-msg ' + tipo;

        const avatarIcon = esBot ? 'bi-robot' : 'bi-person';
        const avatarHtml = '<div class="chatbot-msg-avatar"><i class="bi ' + avatarIcon + '"></i></div>';
        const bubbleHtml = '<div class="chatbot-msg-bubble">' + escapeHtml(texto) + '</div>';

        msgDiv.innerHTML = avatarHtml + bubbleHtml;
        messages.appendChild(msgDiv);
        messages.scrollTop = messages.scrollHeight;
    }

    function mostrarCargando() {
        const loader = document.createElement('div');
        loader.id = 'chatbot-loading';
        loader.className = 'chatbot-msg bot';
        loader.innerHTML =
            '<div class="chatbot-msg-avatar"><i class="bi bi-robot"></i></div>' +
            '<div class="chatbot-msg-bubble">' +
            '<div class="chatbot-loading">' +
            '<div class="chatbot-loading-dot"></div>' +
            '<div class="chatbot-loading-dot"></div>' +
            '<div class="chatbot-loading-dot"></div>' +
            '</div></div>';
        messages.appendChild(loader);
        messages.scrollTop = messages.scrollHeight;
    }

    function eliminarCargando() {
        const loader = document.getElementById('chatbot-loading');
        if (loader) loader.remove();
    }

    function ocultarSugerencias() {
        const sug = document.querySelector('.chatbot-suggestions');
        if (sug) sug.style.display = 'none';
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
})();
