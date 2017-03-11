// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/**
 * Tracks an analytics event.
 * @param {String} category - The event category.
 * @param {String} action - The event action.
 * @param {String} label - The event label.
 * @returns {Boolean} - Whether the analytics event was tracked.
 */
martinCostello.londonTravel.track = function (category, action, label) {
    if ("ga" in window) {
        ga("send", {
            hitType: "event",
            eventCategory: category,
            eventAction: action,
            eventLabel: label
        });
        return true;
    } else {
        return false;
    }
};

(function () {
    $("a, button, input, .ga-track-click").on("click", function () {
        var element = $(this);
        var label = element.attr("data-ga-label") || element.attr("id");
        if (label) {
            martinCostello.londonTravel.track(
                element.attr("data-ga-category") || "General",
                element.attr("data-ga-action") || "clicked",
                label);
        }
    });
})();
