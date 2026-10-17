
$(document).ready(function () {
    const tableSelector = "#liveStockTable";

    if (!$.fn.DataTable) {
        console.error("DataTables is not loaded.");
        return;
    }

    if (!$(tableSelector).length) {
        console.error("Live Stock table was not found.");
        return;
    }

    if ($.fn.DataTable.isDataTable(tableSelector)) {
        $(tableSelector).DataTable().destroy();
    }

    $(tableSelector).DataTable({
        paging: true,
        pageLength: 10,
        lengthMenu: [10, 25, 50, 100],
        searching: true,
        ordering: true,
        scrollY: "350px",
        scrollX: true,
        scrollCollapse: true,
        autoWidth: false,
        language: {
            emptyTable: "No stock records available"
        }
    });
});
