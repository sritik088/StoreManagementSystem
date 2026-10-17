"use strict";

document.addEventListener("DOMContentLoaded", function () {
    const printButton = document.getElementById("printGrnButton");

    if (!printButton) {
        return;
    }

    printButton.addEventListener("click", function () {
        window.print();
    });
});