
document.addEventListener("DOMContentLoaded", function () {
    const printButton = document.getElementById("printPurchaseOrderButton");

    if (printButton) {
        printButton.addEventListener("click", function () {
            window.print();
        });
    }
});
