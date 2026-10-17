
document.addEventListener("DOMContentLoaded", function () {
    const table = document.getElementById("itemTable");
    const tbody = table?.querySelector("tbody");
    const addRowButton = document.getElementById("addRow");
    const form = document.getElementById("purchaseOrderForm");

    if (!table || !tbody || !addRowButton) {
        console.error("Purchase Order item table was not found.");
        return;
    }

    const productInfoUrl = "/PurchaseOrder/GetProductInfo";

    function getNumber(row, selector) {
        const element = row.querySelector(selector);
        return Math.max(0, Number.parseFloat(element?.value) || 0);
    }

    function reindexRows() {
        tbody.querySelectorAll("tr").forEach((row, index) => {
            const fields = [
                [".product", "ProductId"],
                [".product-title", "ProductName"],
                [".product-sku", "SKU"],
                [".qty", "Quantity"],
                [".price", "UnitPrice"],
                [".discount", "DiscountPercent"],
                [".tax", "TaxPercent"],
                [".total", "Total"]
            ];

            fields.forEach(([selector, property]) => {
                const input = row.querySelector(selector);

                if (input) {
                    input.name = `Items[${index}].${property}`;
                }
            });
        });
    }

    function calculateRow(row) {
        const quantity = getNumber(row, ".qty");
        const price = getNumber(row, ".price");
        const discountPercent = Math.min(
            100,
            getNumber(row, ".discount")
        );
        const taxPercent = Math.min(
            100,
            getNumber(row, ".tax")
        );

        const subtotal = quantity * price;
        const discount = subtotal * discountPercent / 100;
        const taxableAmount = Math.max(0, subtotal - discount);
        const tax = taxableAmount * taxPercent / 100;
        const total = taxableAmount + tax;

        const totalInput = row.querySelector(".total");

        if (totalInput) {
            totalInput.value = total.toFixed(2);
        }

        calculateTotals();
    }

    function calculateTotals() {
        let subtotal = 0;
        let discount = 0;
        let tax = 0;
        let grandTotal = 0;

        tbody.querySelectorAll("tr").forEach(row => {
            const quantity = getNumber(row, ".qty");
            const price = getNumber(row, ".price");
            const discountPercent = Math.min(
                100,
                getNumber(row, ".discount")
            );
            const taxPercent = Math.min(
                100,
                getNumber(row, ".tax")
            );

            const lineSubtotal = quantity * price;
            const lineDiscount =
                lineSubtotal * discountPercent / 100;

            const taxableAmount = Math.max(
                0,
                lineSubtotal - lineDiscount
            );

            const lineTax = taxableAmount * taxPercent / 100;
            const lineTotal = taxableAmount + lineTax;

            subtotal += lineSubtotal;
            discount += lineDiscount;
            tax += lineTax;
            grandTotal += lineTotal;

            const totalInput = row.querySelector(".total");

            if (totalInput) {
                totalInput.value = lineTotal.toFixed(2);
            }
        });

        document.getElementById("SubTotal").value =
            subtotal.toFixed(2);

        document.getElementById("Discount").value =
            discount.toFixed(2);

        document.getElementById("TaxAmount").value =
            tax.toFixed(2);

        document.getElementById("GrandTotal").value =
            grandTotal.toFixed(2);
    }

    async function loadProductInfo(row, productId) {
        const titleInput = row.querySelector(".product-title");
        const skuInput = row.querySelector(".product-sku");
        const priceInput = row.querySelector(".price");

        if (!productId) {
            titleInput.value = "";
            skuInput.value = "";
            priceInput.value = "0";
            calculateRow(row);
            return;
        }

        titleInput.value = "Loading...";
        skuInput.value = "";

        try {
            const url = new URL(
                productInfoUrl,
                window.location.origin
            );

            url.searchParams.set("productId", productId);

            const response = await fetch(url.toString(), {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                },
                credentials: "same-origin"
            });

            if (!response.ok) {
                throw new Error(
                    `Product request failed: ${response.status}`
                );
            }

            const data = await response.json();

            // Matches the controller's JSON response:
            // { id, title, sku, barcode, purchasePrice }
            titleInput.value = data.title || "";
            skuInput.value = data.sku || "";

            // The controller currently returns purchasePrice = 0.
            // Replace that server-side logic to load a real price.
            priceInput.value = data.purchasePrice ?? 0;

            calculateRow(row);
        } catch (error) {
            console.error("Unable to load product information:", error);

            titleInput.value = "";
            skuInput.value = "";
            calculateRow(row);

            alert(
                "Unable to load product information. " +
                "Please select the product again."
            );
        }
    }

    function addRow() {
        const firstRow = tbody.querySelector("tr");

        if (!firstRow) {
            return;
        }

        // Clone the existing row, including its server-rendered
        // product dropdown options.
        const newRow = firstRow.cloneNode(true);

        newRow.querySelectorAll("input").forEach(input => {
            if (input.classList.contains("qty")) {
                input.value = "1";
            } else if (
                input.classList.contains("price") ||
                input.classList.contains("discount") ||
                input.classList.contains("tax") ||
                input.classList.contains("total")
            ) {
                input.value = "0";
            } else {
                input.value = "";
            }
        });

        const productSelect = newRow.querySelector(".product");

        if (productSelect) {
            productSelect.selectedIndex = 0;
        }

        tbody.appendChild(newRow);

        reindexRows();
        calculateTotals();
    }

    addRowButton.addEventListener("click", addRow);

    // Delegated events work for both existing and newly added rows.
    tbody.addEventListener("change", function (event) {
        if (!event.target.matches(".product")) {
            return;
        }

        const row = event.target.closest("tr");

        loadProductInfo(row, event.target.value);
    });

    tbody.addEventListener("input", function (event) {
        if (
            event.target.matches(
                ".qty, .price, .discount, .tax"
            )
        ) {
            calculateRow(event.target.closest("tr"));
        }
    });

    tbody.addEventListener("click", function (event) {
        const button = event.target.closest(".removeRow");

        if (!button) {
            return;
        }

        if (tbody.querySelectorAll("tr").length <= 1) {
            alert("At least one product is required.");
            return;
        }

        button.closest("tr").remove();

        reindexRows();
        calculateTotals();
    });

    // Populate title and SKU for any rows that already have a product.
    tbody.querySelectorAll("tr").forEach(row => {
        const productId = row.querySelector(".product")?.value;

        if (productId) {
            loadProductInfo(row, productId);
        }
    });

    reindexRows();
    calculateTotals();

    // Prevent submitting an order without a selected product.
    form?.addEventListener("submit", function (event) {
        const rows = [...tbody.querySelectorAll("tr")];

        const hasProduct = rows.some(row =>
            Number.parseInt(
                row.querySelector(".product")?.value || "0",
                10
            ) > 0
        );

        if (!hasProduct) {
            event.preventDefault();
            alert("Please select at least one product.");
        }
    });
});
