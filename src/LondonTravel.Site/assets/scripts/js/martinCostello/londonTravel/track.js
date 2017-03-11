// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/**
 * Tracks an analytics event.
 * @param {String} category - The event category.
 * @param {String} action - The event action.
 * @param {String} label - The event label.
 * @param {String} [value] - The optional event value.
 * @param {Object} [fields] - The optional event data.
 * @returns {Boolean} - Whether the analytics event was tracked.
 */
martinCostello.londonTravel.track = function (category, action, label, value, fields) {
    if ("ga" in window) {
        ga("send", {
            hitType: "event",
            eventCategory: category,
            eventAction: action,
            eventLabel: label,
            eventValue: value
        });
        return true;
    } else {
        return false;
    }
};

(function () {
    $("a, button, input, .ga-track-click").on("click", function () {
        var element = $(this);
        var label = element.attr("data-ga-label");
        if (label) {
            martinCostello.londonTravel.track(
                element.attr("data-ga-category") || "General",
                element.attr("data-ga-action") || "clicked",
                label,
                element.attr("data-ga-value") || element.attr("id") || "");
        }
    });
})();
