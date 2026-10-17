"use strict";

document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("salesOrderForm");
    const tableBody = document.querySelector("#itemTable tbody");
    const addButton = document.getElementById("addProductBtn");
    const optionsTemplate = document.getElementById("productOptionsTemplate");

    if (!form || !tableBody || !addButton || !optionsTemplate) {
        return;
    }

    const priceUrl = form.dataset.priceUrl || "/SalesOrder/GetProductPrice";

    function numberValue(element) {
        const value = Number.parseFloat(element?.value);
        return Number.isFinite(value) ? value : 0;
    }

    function money(value) {
        return Number(value).toLocaleString("en-IN", {
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        });
    }

    function currency(value) {
        return "₹" + money(value);
    }

    function getRowValues(row) {
        const qty = numberValue(row.querySelector(".qty"));
        const cost = numberValue(row.querySelector(".cost-price"));
        const salePrice = numberValue(row.querySelector(".sale-price"));
        const taxPercent = numberValue(row.querySelector(".tax-percent"));
        const discountPercent = numberValue(row.querySelector(".discount-percent"));

        const grossSales = qty * salePrice;
        const discount = grossSales * discountPercent / 100;
        const netSales = Math.max(0, grossSales - discount);
        const tax = netSales * taxPercent / 100;
        const total = netSales + tax;
        const costAmount = qty * cost;
        const profit = netSales - costAmount;

        return {
            grossSales,
            discount,
            netSales,
            tax,
            total,
            costAmount,
            profit
        };
    }

    function calculateRow(row) {
        if (!row) return;

        const values = getRowValues(row);

        row.querySelector(".line-total").value = values.total.toFixed(2);
        row.querySelector(".line-profit").value = values.profit.toFixed(2);

        calculateTotals();
    }

    function calculateTotals() {
        let subTotal = 0;
        let discount = 0;
        let tax = 0;
        let grandTotal = 0;
        let grossProfit = 0;

        tableBody.querySelectorAll(".item-row").forEach(function (row) {
            const values = getRowValues(row);

            subTotal += values.grossSales;
            discount += values.discount;
            tax += values.tax;
            grandTotal += values.total;
            grossProfit += values.profit;
        });

        document.getElementById("SubTotal").value = subTotal.toFixed(2);
        document.getElementById("DiscountAmount").value = discount.toFixed(2);
        document.getElementById("TaxAmount").value = tax.toFixed(2);
        document.getElementById("GrandTotal").value = grandTotal.toFixed(2);
        document.getElementById("GrossProfit").value = grossProfit.toFixed(2);

        document.getElementById("subTotalDisplay").textContent = currency(subTotal);
        document.getElementById("discountDisplay").textContent = currency(discount);
        document.getElementById("taxDisplay").textContent = currency(tax);
        document.getElementById("grandTotalDisplay").textContent = currency(grandTotal);
        document.getElementById("grossProfitDisplay").textContent = currency(grossProfit);
    }

    function reindexRows() {
        tableBody.querySelectorAll(".item-row").forEach(function (row, index) {
            row.querySelector(".product-select").name = `Items[${index}].ProductId`;
            row.querySelector(".qty").name = `Items[${index}].Quantity`;
            row.querySelector(".cost-price").name = `Items[${index}].UnitPrice`;
            row.querySelector(".sale-price").name = `Items[${index}].SalePrice`;
            row.querySelector(".tax-percent").name = `Items[${index}].TaxPercent`;
            row.querySelector(".discount-percent").name = `Items[${index}].DiscountPercent`;
        });
    }

    function addProductRow() {
        const index = tableBody.querySelectorAll(".item-row").length;
        const row = document.createElement("tr");

        row.className = "item-row";

        row.innerHTML = `
            <td>
                <select class="form-select product-select"
                        name="Items[${index}].ProductId"
                        required>
                </select>
            </td>
            <td>
                <input type="number" class="form-control qty"
                       name="Items[${index}].Quantity"
                       value="1" min="0.01" step="0.01" required>
            </td>
            <td>
                <input type="number" class="form-control cost-price"
                       name="Items[${index}].UnitPrice"
                       value="0.00" readonly>
            </td>
            <td>
                <input type="number" class="form-control sale-price"
                       name="Items[${index}].SalePrice"
                       value="0.00" min="0" step="0.01" required>
            </td>
            <td>
                <input type="number" class="form-control tax-percent"
                       name="Items[${index}].TaxPercent"
                       value="0" min="0" max="100" step="0.01" required>
            </td>
            <td>
                <input type="number" class="form-control discount-percent"
                       name="Items[${index}].DiscountPercent"
                       value="0" min="0" max="100" step="0.01" required>
            </td>
            <td>
                <input type="text" class="form-control line-total"
                       value="0.00" readonly>
            </td>
            <td>
                <input type="text" class="form-control line-profit"
                       value="0.00" readonly>
            </td>
            <td class="text-center">
                <button type="button"
                        class="btn btn-outline-danger btn-sm delete-row"
                        aria-label="Remove product"
                        title="Remove product">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        `;

        const select = row.querySelector(".product-select");

        // Clone server-rendered options so product titles remain safely encoded.
        select.appendChild(optionsTemplate.content.cloneNode(true));

        tableBody.appendChild(row);
        reindexRows();
        calculateRow(row);
    }

    async function loadProductPrice(select) {
        const row = select.closest(".item-row");
        const costInput = row.querySelector(".cost-price");
        const productId = select.value;

        costInput.value = "0.00";

        if (!productId) {
            calculateRow(row);
            return;
        }

        try {
            const url = new URL(priceUrl, window.location.origin);
            url.searchParams.set("productId", productId);

            const response = await fetch(url.toString(), {
                method: "GET",
                headers: { "Accept": "application/json" },
                credentials: "same-origin"
            });

            if (!response.ok) {
                throw new Error("Unable to retrieve product price.");
            }

            const data = await response.json();

            if (data.success && Number.isFinite(Number(data.unitPrice))) {
                costInput.value = Number(data.unitPrice).toFixed(2);
            }

            calculateRow(row);
        } catch (error) {
            console.error("Product price lookup failed:", error);
            calculateRow(row);
        }
    }

    addButton.addEventListener("click", addProductRow);

    tableBody.addEventListener("change", function (event) {
        if (event.target.classList.contains("product-select")) {
            loadProductPrice(event.target);
        }
    });

    tableBody.addEventListener("input", function (event) {
        if (
            event.target.matches(
                ".qty, .sale-price, .tax-percent, .discount-percent"
            )
        ) {
            calculateRow(event.target.closest(".item-row"));
        }
    });

    tableBody.addEventListener("click", function (event) {
        const button = event.target.closest(".delete-row");
        if (!button) return;

        const rows = tableBody.querySelectorAll(".item-row");

        if (rows.length <= 1) {
            window.alert("At least one product is required.");
            return;
        }

        button.closest(".item-row").remove();
        reindexRows();
        calculateTotals();
    });

    // Ensure at least one product row exists.
    if (tableBody.querySelectorAll(".item-row").length === 0) {
        addProductRow();
    } else {
        tableBody.querySelectorAll(".item-row").forEach(calculateRow);
    }
});