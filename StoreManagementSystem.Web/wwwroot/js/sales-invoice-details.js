
"use strict";

document.addEventListener("DOMContentLoaded", function () {
    document.querySelectorAll("form[data-confirm-message]").forEach(function (form) {
        form.addEventListener("submit", function (event) {
            const message = form.getAttribute("data-confirm-message");

            if (message && !window.confirm(message)) {
                event.preventDefault();
            }
        });
    });
});
