(function () {
    $("#delete-account")
        .fadeIn("slow")
        .removeClass("hide");
    $("#delete-account-js").on("submit", function () {
        $(".js-delete-control").addClass("disabled");
        $(".js-delete-content").addClass("hide");
        $(".js-delete-loader").removeClass("hide");
    });
    $(".delete-account-modal").on("show.bs.modal", function () {
        setTimeout(function () {
            $(".js-delete-confirm")
                .prop("disabled", false)
                .removeClass("disabled");
        }, 3000);
    });
})();
