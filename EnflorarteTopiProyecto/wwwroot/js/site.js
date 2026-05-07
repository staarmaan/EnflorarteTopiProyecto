// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// Jenny: Logica de comandas en dirección y anticipos
$(document).ready(function () {

    // Verificamos si existe el formulario de comandas en esta página
    // Si no existe, no hace nada (para no causar errores en otras pantallas)
    if ($('#selectTipoEntrega').length) {

        function toggleAnticipo() { //Para el tipo de anticipo
            var texto = $('#selectTipoAnticipo option:selected').text();
            var $input = $('#inputAnticipoValor');
            var $label = $('#labelAnticipoValor');

            if (texto.toLowerCase().includes('porcentaje')) {
                $label.text('Porcentaje del anticipo (%)');
                $input.attr('max', '100');

            } else {
                $label.text('Monto del anticipo ($)');
                $input.removeAttr('max');
            }
        }

        // Asignamos los eventos
        $('#selectTipoAnticipo').change(toggleAnticipo);

        // Ejecuta al entrar a la página para aplicar el estado inicial
        toggleAnticipo();
    }
});