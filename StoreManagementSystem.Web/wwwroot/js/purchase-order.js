$.get("/PurchaseOrder/GetProductInfo",
    {
        productId: productId
    },
    function (product) {

        row.find(".price").val(product.purchasePrice);

        row.find(".tax").val(product.taxRate);

        row.find(".sku").text(product.sku);

        row.find(".unit").text(product.unitName);

        row.find(".stock").text(product.currentStock);

        row.find(".productImage")
            .attr("src",
                product.image ??
                "/images/no-image.png");

        calculateRow(row);

    });

$(document).on("change", ".product", function () {

    let selected = [];

    $(".product").each(function () {

        if ($(this).val() != "")

            selected.push($(this).val());

    });

    let unique = [...new Set(selected)];

    if (selected.length != unique.length) {

        toastr.error("Product already added.");

        $(this).val("");

    }

});

if (qty > stock) {

    row.addClass("table-warning");

}
else {

    row.removeClass("table-warning");

}