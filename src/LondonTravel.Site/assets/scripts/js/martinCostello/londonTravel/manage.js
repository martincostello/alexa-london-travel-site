(function () {
    $(".js-hidden-control")
        .fadeIn("slow")
        .removeClass("hide");
    $(".js-modal-form").on("submit", function () {
        var form = $(this);
        form.find(".js-delete-control").addClass("disabled");
        form.find(".js-delete-content").addClass("hide");
        form.find(".js-delete-loader").removeClass("hide");
    });
    $(".js-modal").on("show.bs.modal", function () {
        var modal = $(this);
        setTimeout(function () {
            modal.find(".js-modal-confirm")
                .prop("disabled", false)
                .removeClass("disabled");
        }, 3000);
    });
})();
