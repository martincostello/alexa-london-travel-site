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
        ga("send", "event", category, action, label, value, fields);
        return true;
    } else {
        return false;
    }
};
