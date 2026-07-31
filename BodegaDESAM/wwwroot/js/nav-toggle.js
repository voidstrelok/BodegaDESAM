// Mobile Navigation Toggle
// Maneja el comportamiento del menú móvil

(function() {
    'use strict';

    // Cerrar menú al hacer clic en un enlace (móvil)
    document.addEventListener('DOMContentLoaded', function() {
        const navToggle = document.getElementById('nav-toggle');
        const navLinks = document.querySelectorAll('.nav-scrollable .nav-link');
        
        if (!navToggle) return;

        // Cerrar menú al hacer clic en cualquier enlace
        navLinks.forEach(link => {
            link.addEventListener('click', function() {
                // Solo en móvil
                if (window.innerWidth <= 641) {
                    navToggle.checked = false;
                }
            });
        });

        // Cerrar menú al hacer clic fuera del sidebar (overlay)
        const navScrollable = document.querySelector('.nav-scrollable');
        if (navScrollable) {
            document.addEventListener('click', function(e) {
                if (window.innerWidth <= 641 && 
                    navToggle.checked && 
                    !navScrollable.contains(e.target) && 
                    !e.target.classList.contains('navbar-toggler-btn') &&
                    !e.target.closest('.navbar-toggler-btn')) {
                    navToggle.checked = false;
                }
            });
        }

        // Cerrar menú al presionar Escape
        document.addEventListener('keydown', function(e) {
            if (e.key === 'Escape' && navToggle.checked) {
                navToggle.checked = false;
            }
        });
    });

    // Conectar el botón toggle con el checkbox
    const toggleBtn = document.querySelector('.navbar-toggler-btn');
    const navToggle = document.getElementById('nav-toggle');
    
    if (toggleBtn && navToggle) {
        toggleBtn.addEventListener('click', function(e) {
            e.stopPropagation();
            navToggle.checked = !navToggle.checked;
        });
    }
})();
