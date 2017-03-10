(function () {
    var setupDelete = function () {
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
    };
    if ("jQuery" in window) {
        setupDelete();
    }
    else {
        setTimeout(setupDelete, 1000);
    }
})();
