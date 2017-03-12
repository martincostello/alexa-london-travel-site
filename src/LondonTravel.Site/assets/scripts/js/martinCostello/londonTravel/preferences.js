// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/**
 * Defines the namespace for line preferences.
 */
martinCostello.londonTravel.preferences = {
};

/**
 * Handles a line being clicked.
 */
martinCostello.londonTravel.preferences.onLineClicked = function () {
    var element = $(this);
    var checkbox = element.find("input[type='checkbox']");
    checkbox.prop("checked", !checkbox.prop("checked"));
};

(function () {
    $(".js-travel-line").on("click", martinCostello.londonTravel.preferences.onLineClicked);
})();
