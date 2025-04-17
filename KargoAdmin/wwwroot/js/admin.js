// Toggle sidebar
window.addEventListener('DOMContentLoaded', event => {
    const sidebarToggle = document.body.querySelector('#sidebarToggle');
    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', event => {
            event.preventDefault();
            document.body.classList.toggle('sb-sidenav-toggled');
            localStorage.setItem('sb|sidebar-toggle', document.body.classList.contains('sb-sidenav-toggled'));
        });
    }
});

// Initialize Simple DataTables if exists on page
document.addEventListener('DOMContentLoaded', () => {
    const datatablesSimple = document.getElementById('blogsTable');
    if (datatablesSimple) {
        new simpleDatatables.DataTable(datatablesSimple);
    }
});