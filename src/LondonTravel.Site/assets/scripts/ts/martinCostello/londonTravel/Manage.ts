// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.londonTravel {

    /**
     * Represents the class for managing accounts.
     */
    export class Manage {

        private readonly confirmationTimeout: number;

        /**
         * Initializes a new instance of the martinCostello.londonTravel.Manage class.
         * @constructor
         */
        public constructor() {
            this.confirmationTimeout = 3000;
        }

        /**
         * Initializes the page.
         */
        public initialize(): void {
            $(".js-hidden-control")
                .fadeIn()
                .removeClass("hide");
            $(".js-modal-form").on("submit", this.onFormSubmit);
            $(".js-modal").on("show.bs.modal", this.onModalShown);
        }

        /**
         * Handles the form being submitted.
         * @param {JQueryEventObject} e - The event object.
         */
        private onFormSubmit = (e: JQuery.Event): void => {
            let form: JQuery = $(e.target);
            form.find(".js-delete-control").addClass("disabled");
            form.find(".js-delete-content").addClass("hide");
            form.find(".js-delete-loader").removeClass("hide");
        }

        /**
         * Handles the modal being displayed.
         * @param {JQueryEventObject} e - The event object.
         */
        private onModalShown = (e: JQuery.Event): void => {
            let modal: JQuery = $(e.target);
            setTimeout(() => {
                modal.find(".js-modal-confirm")
                    .prop("disabled", false)
                    .removeClass("disabled");
            }, this.confirmationTimeout);
        }
    }
}
((): any => {
    let manage: martinCostello.londonTravel.Manage = new martinCostello.londonTravel.Manage();
    manage.initialize();
})();
