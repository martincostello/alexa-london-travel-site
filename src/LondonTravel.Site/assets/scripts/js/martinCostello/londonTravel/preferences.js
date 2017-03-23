// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/**
 * Initializes a new instance of the martinCostello.londonTravel.preferences class.
 * @constructor
 */
martinCostello.londonTravel.preferences = function () {

    this.container = $(".js-preferences-container");
    this.initialState = this.getCurrentState();

    this.clearButton = this.container.find(".js-preferences-clear");
    this.resetButton = this.container.find(".js-preferences-reset");
    this.saveButton = this.container.find(".js-preferences-save");

    this.favoritesCount = this.container.find(".js-favorites-count").removeClass("hide");
    this.otherCount = this.container.find(".js-other-count").removeClass("hide");

    // Treat entire div as a button
    this.container
        .find(".js-travel-line")
        .on("click", $.proxy(this, "onLineClicked"));

    // Show the reset button to restore initial preferences' state
    this.resetButton
        .on("click", $.proxy(this, "resetFavoriteLines"))
        .addClass("disabled")
        .prop("disabled", true)
        .removeClass("hide");

    // Disable the clear button if everything is currently deselected
    if (this.initialState === "") {
        this.clearButton
            .addClass("disabled")
            .prop("disabled", true);
    }

    // Add handler for clearing the selections and show the button
    this.clearButton
        .on("click", $.proxy(this, "clearFavoriteLines"))
        .removeClass("hide");

    // Disable the save button the state is different
    this.saveButton
        .addClass("disabled")
        .prop("disabled", true);

    // Check the form's state whenever an option is changed
    this.container
        .find(".js-line-preference")
        .on("change", $.proxy(this, "checkCurrentState"));

    // Dismiss any visible success alerts
    setTimeout(function () {
        $(".alert-success").slideUp(1000);
    }, 3000);
};

/**
 * Sets the state of the specified button to be enabled or not.
 * @param {Object} button - The button to enable or disable.
 * @param {Boolean} enabled - Whether the button is enabled, or not.
 */
martinCostello.londonTravel.preferences.prototype.setButtonState = function (button, enabled) {
    if (enabled === true) {
        button.prop("disabled", false).removeClass("disabled");
    }
    else {
        button.prop("disabled", true).addClass("disabled");
    }
};

/**
 * Checks the current state of the preferences form.
 */
martinCostello.londonTravel.preferences.prototype.checkCurrentState = function () {

    var currentState = this.getCurrentState();
    var isDirty = this.initialState !== currentState;

    this.setButtonState(this.resetButton, isDirty);
    this.setButtonState(this.saveButton, isDirty);
    this.setButtonState(this.clearButton, currentState !== "");

    var total = this.getAllCheckboxes().length;
    var favorites = this.getSelectedCheckboxes().length;
    var others = total - favorites;

    this.favoritesCount.text("(" + favorites.toString(10) + ")");
    this.otherCount.text("(" + others.toString(10) + ")");
};

/**
 * Clears all of the selected lines.
 */
martinCostello.londonTravel.preferences.prototype.clearFavoriteLines = function () {

    this.getSelectedCheckboxes()
        .prop("checked", false);

    this.checkCurrentState();
};

/**
 * Resets the selected lines.
 */
martinCostello.londonTravel.preferences.prototype.resetFavoriteLines = function () {

    var ids = this.initialState.split(",");

    this.getAllCheckboxes()
        .each(function () {
            var element = $(this);
            var id = element.attr("data-line-id");
            element.prop("checked", ids.indexOf(id) > -1);
        });

    this.checkCurrentState();
};

/**
 * Handles a line being clicked.
 * @param {Object} e - The event arguments.
 */
martinCostello.londonTravel.preferences.prototype.onLineClicked = function (e) {

    var element = $(e.target);
    var checkbox = element.find("input[type='checkbox']");
    checkbox.prop("checked", !checkbox.prop("checked"));

    this.checkCurrentState();
};

/**
 * Gets the current state of the preferences.
 * @returns {String} - The current state of the preferences.
 */
martinCostello.londonTravel.preferences.prototype.getCurrentState = function () {

    var ids = [];

    this.getSelectedCheckboxes()
        .each(function () {
            var element = $(this);
            ids.push(element.attr("data-line-id"));
        });

    ids.sort();

    return ids.join();
};

/**
 * Gets all of the checkboxes in the preferences control.
 * @returns {Object} - The jQuery element containing all the checkboxes.
 */
martinCostello.londonTravel.preferences.prototype.getAllCheckboxes = function () {
    return this.container.find("input[type='checkbox']");
};

/**
 * Gets the selected checkboxes in the preferences control.
 * @returns {Object} - The jQuery element containing the selected checkboxes.
 */
martinCostello.londonTravel.preferences.prototype.getSelectedCheckboxes = function () {
    return this.container.find("input[type='checkbox']:checked");
};

(function () {
    var preferences = new martinCostello.londonTravel.preferences();
})();
