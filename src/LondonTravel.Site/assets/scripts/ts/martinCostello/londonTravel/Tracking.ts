// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.londonTravel {

    /**
     * Represents the class for analytics tracking.
     */
    export class Tracking {

        /**
         * Tracks an analytics event.
         * @param {string} category - The event category.
         * @param {string} action - The event action.
         * @param {string} label - The event label.
         * @returns {boolean} - Whether the analytics event was tracked.
         */
        public static track(category: string, action: string, label: string): boolean {

            let tracked: boolean = false;

            if ("ga" in window && ga) {

                const command: string = "send";
                const fields: any = {
                    hitType: "event",
                    eventCategory: category,
                    eventAction: action,
                    eventLabel: label
                };

                ga(command, fields);

                tracked = true;
            }

            return tracked;
        }
    }
}

(() => {
    $("a, button, input, .ga-track-click").on("click", (e: JQuery.Event): void => {

        const element = $(e.target);
        const label: string = element.attr("data-ga-label") || element.attr("id");

        if (label) {

            const category: string = element.attr("data-ga-category") || "General";
            const action: string = element.attr("data-ga-action") || "clicked";

            martinCostello.londonTravel.Tracking.track(
                category,
                action,
                label);
        }
    });
})();
