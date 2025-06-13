document.addEventListener('DOMContentLoaded', function () {
    const usuarioId = localStorage.getItem('usuarioId');
    const form = document.getElementById('transaccionForm');
    const responseBox = document.getElementById('responseTransaccion');

    // Mapeo correcto según los valores del HTML
    const criptoIdMap = { '1': 'BTC', '2': 'ETH', '3': 'USDC' };
    const mercadoIdMap = { '1': 'Binance', '2': 'Buenbit', '3': 'LemonCash' };

    if (!usuarioId) {
        if (responseBox) responseBox.textContent = "❌ Debes iniciar sesión.";
        if (form) form.querySelectorAll('input, select, button').forEach(el => el.disabled = true);
        return;
    }

    if (form) {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            const criptoId = document.getElementById('cripto').value;
            const mercadoId = document.getElementById('mercado').value;
            const operacion = document.getElementById('operacion').value;
            let cantidad = parseFloat(document.getElementById('cantidad').value);

            // Validaciones
            if (isNaN(cantidad) || cantidad === null || cantidad === undefined) {
                if (responseBox) responseBox.textContent = "❌ Ingresa una cantidad válida.";
                responseBox.textContent = "❌ Ingresa una cantidad válida.";
                return;
            }

            cantidad = Number(cantidad.toFixed(8));
            if (cantidad <= 0) {
                responseBox.textContent = "❌ Ingresa una cantidad válida mayor a 0.";
                return;
            }

            if (cantidad > 999999999999999999.99) {
                responseBox.textContent = "❌ La cantidad máxima es 999999999999999999.99";
                return;
            }

            const body = {
                UsuarioId: Number(usuarioId),
                CriptoId: Number(criptoId),
                MercadoId: Number(mercadoId),
                Operacion: operacion,
                CantCripto: cantidad,
                Fecha: new Date().toISOString()
            };

            console.log("Body enviado:", JSON.stringify(body));

            try {
                const res = await fetch('https://localhost:7242/api/Transaccion/Transaccion', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(body)
                });

                const text = await res.text();
                let data;
                try {
                    data = JSON.parse(text);
                } catch {
                    data = text;
                }

                if (res.ok) {
                    const criptoNombre = document.getElementById('cripto').options[document.getElementById('cripto').selectedIndex].text;
                    const mercadoNombre = document.getElementById('mercado').options[document.getElementById('mercado').selectedIndex].text;
                    
                    responseBox.textContent = `✅ ${operacion === 'compra' ? 'Compraste' : 'Vendiste'} ${cantidad} ${criptoNombre} en ${mercadoNombre}.`;
                    form.reset();
                } else {
                    responseBox.textContent = `❌ Error: ${data.message || data}`;
                }
            } catch (error) {
                responseBox.textContent = "❌ Error de conexión con el servidor.";
                console.error("Error en fetch:", error);
            }
        });
    }
});