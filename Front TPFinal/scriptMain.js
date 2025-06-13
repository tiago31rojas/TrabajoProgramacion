document.addEventListener('DOMContentLoaded', function() {
    const logoutBtn = document.getElementById('logoutBtn');
    if (logoutBtn) {
        logoutBtn.onclick = function() {
            localStorage.removeItem('usuarioId');
            localStorage.removeItem('usuarioNombre');
            window.location.href = "indexLogin.html";
        };
    }
});