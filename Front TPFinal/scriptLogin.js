if (localStorage.getItem('usuarioId')) {
    window.location.href = "indexHome.html";
}

document.getElementById("loginForm").addEventListener("submit", async function (e) {
    e.preventDefault();

    const gmail = document.getElementById("gmail").value;
    const password = document.getElementById("password").value;
    const responseBox = document.getElementById("response");

    try {
        const res = await fetch("https://localhost:7242/api/Registro/login", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ Gmail: gmail, Contrasenia: password })
        });

        const text = await res.text();
        let data;
        try {
            data = JSON.parse(text);
        } catch {
            data = text;
        }

        if (res.ok) {
            
            localStorage.setItem("usuarioId", data.id || data.Id || data.usuarioId);
            localStorage.setItem("usuarioNombre", data.nombre || data.Nombre || gmail);

            responseBox.className = "response success";
            responseBox.innerText = `✅ Bienvenido. Redirigiendo...`;

            setTimeout(() => {
                window.location.href = "indexHome.html";
            }, 1000);
        } else {
            responseBox.className = "response error";
            responseBox.innerText = `❌ ${data}`;
        }
    } catch (error) {
        responseBox.className = "response error";
        responseBox.innerText = "❌ Error al conectar con el servidor.";
        console.error(error);
    }
});

// Botón para ir a registro
document.getElementById('btnRegister').onclick = function() {
    window.location.href = "indexRegister.html";
};