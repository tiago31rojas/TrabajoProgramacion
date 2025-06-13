let chartBinance, chartBuenbit, chartLemnoCash;

const usuarioId = localStorage.getItem('usuarioId');
const nombreUsuario = localStorage.getItem('usuarioNombre') || 'Usuario';

document.addEventListener('DOMContentLoaded', function() {
    const nombreUsuarioElem = document.getElementById('nombreUsuario');
    if (nombreUsuarioElem) {
        nombreUsuarioElem.textContent = nombreUsuario;
    }
});

async function cargarHistorial() {
    if (!usuarioId) return;
    document.getElementById('historialLoading').style.display = 'block';
    document.getElementById('historialVacio').style.display = 'none';
    const tbody = document.getElementById('historialBody');
    tbody.innerHTML = '';
    try {
      const res = await fetch(`https://localhost:7242/api/Transaccion/Historial?usuarioid=${usuarioId}`);
      if (res.ok) {
        const historial = await res.json();
        if (historial.length === 0) {
          document.getElementById('historialVacio').style.display = 'block';
        } else {
          historial.forEach(t => {
            const fecha = new Date(t.fecha).toLocaleString('es-AR');
            
            const cantCriptoValue = parseFloat(t.cantCripto);
            const cantCriptoFormatted = !isNaN(cantCriptoValue) 
              ? cantCriptoValue.toLocaleString('es-AR', {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2,
                  useGrouping: true
                })
              : '0,00';
            
            const cantPesosValue = parseFloat(t.cantPesos);
            const cantPesosFormatted = !isNaN(cantPesosValue)
              ? cantPesosValue.toLocaleString('es-AR', {
                  minimumFractionDigits: 2,
                  maximumFractionDigits: 2,
                  useGrouping: true
                })
              : '0,00';

            const fila = `<tr>
              <td>${fecha}</td>
              <td>${t.operacion.charAt(0).toUpperCase() + t.operacion.slice(1)}</td>
              <td>${t.criptoNombre}</td>
              <td>${t.mercadoNombre}</td>
              <td class="text-right">${cantCriptoFormatted}</td>
              <td class="text-right">$${cantPesosFormatted}</td>
            </tr>`;
            tbody.innerHTML += fila;
          });
        }
      } else {
        document.getElementById('historialVacio').textContent = "Error al cargar el historial.";
        document.getElementById('historialVacio').style.display = 'block';
      }
    } catch {
      document.getElementById('historialVacio').textContent = "Error de conexión al cargar el historial.";
      document.getElementById('historialVacio').style.display = 'block';
    }
    document.getElementById('historialLoading').style.display = 'none';
}
cargarHistorial();

async function mostrarGraficosCripto() {
    const mercados = ["Binance", "Buenbit", "LemnoCash"];
    const criptos = ["BTC", "ETH", "USDC"];
    const precios = {
      Binance: [0, 0, 0],
      Buenbit: [0, 0, 0],
      LemnoCash: [0, 0, 0]
    };

    try {
      const res = await fetch("https://localhost:7242/api/PreciosCripto/precios");
      if (res.ok) {
        const data = await res.json();
        mercados.forEach((mercado, i) => {
          criptos.forEach((cripto, j) => {
            const item = data.find(d => d.mercado === mercado && d.cripto === cripto);
            precios[mercado][j] = item && item.precioCompra ? item.precioCompra : 0;
          });
        });
      }
    } catch {
    }

    if (!chartBinance) {
      chartBinance = new Chart(document.getElementById('chartBinance').getContext('2d'), {
        type: 'bar',
        data: {
          labels: criptos,
          datasets: [{
            label: 'Precio en Binance (ARS)',
            data: precios.Binance,
            backgroundColor: [
              'rgba(255, 206, 86, 0.7)',
              'rgba(54, 162, 235, 0.7)',
              'rgba(75, 192, 192, 0.7)'
            ],
            borderColor: [
              'rgba(255, 206, 86, 1)',
              'rgba(54, 162, 235, 1)',
              'rgba(75, 192, 192, 1)'
            ],
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true, max: 2000 } }
        }
      });
    } else {
      chartBinance.data.datasets[0].data = precios.Binance;
      chartBinance.update();
    }

    if (!chartBuenbit) {
      chartBuenbit = new Chart(document.getElementById('chartBuenbit').getContext('2d'), {
        type: 'bar',
        data: {
          labels: criptos,
          datasets: [{
            label: 'Precio en Buenbit (ARS)',
            data: precios.Buenbit,
            backgroundColor: [
              'rgba(255, 206, 86, 0.5)',
              'rgba(54, 162, 235, 0.5)',
              'rgba(75, 192, 192, 0.5)'
            ],
            borderColor: [
              'rgba(255, 206, 86, 1)',
              'rgba(54, 162, 235, 1)',
              'rgba(75, 192, 192, 1)'
            ],
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true, max: 2000 } }
        }
      });
    } else {
      chartBuenbit.data.datasets[0].data = precios.Buenbit;
      chartBuenbit.update();
    }

    if (!chartLemnoCash) {
      chartLemnoCash = new Chart(document.getElementById('chartLemonCash').getContext('2d'), {
        type: 'bar',
        data: {
          labels: criptos,
          datasets: [{
            label: 'Precio en LemnoCash (ARS)',
            data: precios.LemnoCash,
            backgroundColor: [
              'rgba(255, 206, 86, 0.3)',
              'rgba(54, 162, 235, 0.3)',
              'rgba(75, 192, 192, 0.3)'
            ],
            borderColor: [
              'rgba(255, 206, 86, 1)',
              'rgba(54, 162, 235, 1)',
              'rgba(75, 192, 192, 1)'
            ],
            borderWidth: 1
          }]
        },
        options: {
          responsive: true,
          plugins: { legend: { display: false } },
          scales: { y: { beginAtZero: true, max: 1000 } }
        }
      });
    } else {
      chartLemnoCash.data.datasets[0].data = precios.LemnoCash;
      chartLemnoCash.update();
    }
}
mostrarGraficosCripto();
setInterval(mostrarGraficosCripto, 10000);