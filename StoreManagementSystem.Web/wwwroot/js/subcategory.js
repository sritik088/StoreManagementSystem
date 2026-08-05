$(document).ready(function () {

    // Initialize DataTable
    $("#subCategoryTable").DataTable({

        responsive: true,

        pageLength: 10,

        lengthMenu: [
            [10, 25, 50, 100],
            [10, 25, 50, 100]
        ],

        ordering: true,

        searching: true,

        paging: true,

        info: true,

        autoWidth: false,

        dom: 'Bfrtip',

        buttons: [

            {
                extend: 'copy',
                className: 'btn btn-secondary btn-sm'
            },

            {
                extend: 'excel',
                className: 'btn btn-success btn-sm'
            },

            {
                extend: 'pdf',
                className: 'btn btn-danger btn-sm'
            },

            {
                extend: 'print',
                className: 'btn btn-primary btn-sm'
            }

        ]

    });

    // SweetAlert Delete
    $(".deleteForm").submit(function (e) {

        e.preventDefault();

        let form = this;

        Swal.fire({

            title: "Delete Sub Category?",

            text: "This action cannot be undone.",

            icon: "warning",

            showCancelButton: true,

            confirmButtonColor: "#dc3545",

            cancelButtonColor: "#6c757d",

            confirmButtonText: "Yes, Delete",

            cancelButtonText: "Cancel"

        }).then((result) => {

            if (result.isConfirmed) {

                form.submit();

            }

        });

    });

    // Auto-hide success alert
    setTimeout(function () {

        $(".alert-success").fadeOut("slow");

    }, 3000);

});