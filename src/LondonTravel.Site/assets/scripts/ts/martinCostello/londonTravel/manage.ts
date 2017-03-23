// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/// <reference path="../../../../../node_modules/@types/jquery/index.d.ts" />

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
         * @param {Object} e - The event.
         */
        private onFormSubmit = (e: any): void => {
            let form: JQuery = $(e.target);
            form.find(".js-delete-control").addClass("disabled");
            form.find(".js-delete-content").addClass("hide");
            form.find(".js-delete-loader").removeClass("hide");
        }

        /**
         * Handles the modal being displayed.
         * @param {Object} e - The event.
         */
        private onModalShown = (e: any): void => {
            let modal: JQuery = $(e.target);
            setTimeout(() => {
                modal.find(".js-modal-confirm")
                    .prop("disabled", false)
                    .removeClass("disabled");
            }, this.confirmationTimeout);
        }
    }
}
(function(): any {
    let manage: martinCostello.londonTravel.Manage = new martinCostello.londonTravel.Manage();
    manage.initialize();
})();
