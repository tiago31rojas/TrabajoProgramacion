document.addEventListener('DOMContentLoaded', function() {
    const nombreUsuarioElem = document.getElementById('nombreUsuario');
    const saldoPesosElem = document.getElementById('saldoPesos');
    const usuarioId = localStorage.getItem('usuarioId');
    if (nombreUsuarioElem) {
        nombreUsuarioElem.textContent = localStorage.getItem('usuarioNombre') || 'Usuario';
    }

    async function cargarSaldos() {
        if (!usuarioId) return;
        try {
            const res = await fetch(`https://localhost:7242/api/Billetera/saldos/${usuarioId}`);
            if (res.ok) {
                const data = await res.json();
                if (saldoPesosElem) {
                    saldoPesosElem.textContent = Number(data.saldoPesos).toLocaleString('es-AR');
                }
            } else {
                if (saldoPesosElem) saldoPesosElem.textContent = "0";
            }
        } catch {
            if (saldoPesosElem) saldoPesosElem.textContent = "0";
        }
    }
    cargarSaldos();

    const billeteraForm = document.getElementById('billeteraForm');
    if (billeteraForm) {
        billeteraForm.addEventListener('submit', async function(e) {
            e.preventDefault();
            const monto = document.getElementById('montoPesos').value;
            const responseBox = document.getElementById('responseBilletera');
            if (!usuarioId) {
                if (responseBox) responseBox.textContent = "❌ Debes iniciar sesión.";
                return;
            }
            try {
                const res = await fetch('https://localhost:7242/api/Billetera/transferir', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ usuarioId: Number(usuarioId), pesos: Number(monto) })
                });
                if (res.ok) {
                    if (responseBox) responseBox.textContent = `✅ Se transfirieron $${monto} a tu billetera.`;
                    await cargarSaldos();
                } else {
                    const errorMsg = await res.text();
                    if (responseBox) responseBox.textContent = "❌ Error al transferir: " + errorMsg;
                }
            } catch {
                if (responseBox) responseBox.textContent = "❌ Error de conexión.";
            }
            this.reset();
        });
    }
});