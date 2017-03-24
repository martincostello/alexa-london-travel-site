// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

describe("Given the namespaces are defined", function () {

    it("then martinCostello is not null", function () {
        expect(martinCostello).not.toBeNull();
    });

    it("then martinCostello.londonTravel is not null", function () {
        expect(martinCostello.londonTravel).not.toBeNull();
    });
});

describe("Google Analytics", function () {

    describe("Given ga is not defined", function () {

        it("then an event is not published", function () {

            var category = "category";
            var action = "action";
            var label = "label";

            var result = martinCostello.londonTravel.Tracking.track(category, action, label);

            expect(result).toBe(false);
        });
    });

    describe("Given ga is defined", function () {

        beforeEach(function () {
            ga = jasmine.createSpy("ga");
        });

        afterEach(function () {
            ga = null;
        });

        it("then an event is published", function () {

            var category = "category";
            var action = "action";
            var label = "label";

            var result = martinCostello.londonTravel.Tracking.track(category, action, label);

            expect(result).toBe(true);
            expect(ga).toHaveBeenCalledWith("send", jasmine.objectContaining({
                hitType: "event",
                eventCategory: category,
                eventAction: action,
                eventLabel: label
            }));
        });
    });
});

describe("Debugging", function () {

    describe("Given meta tags containing the site version", function () {

        beforeAll(function () {
            $("head").append("<meta name='x-site-branch'] content='master' />");
            $("head").append("<meta name='x-site-revision'] content='012345ab' />");
        });

        it("then the branch is correct", function () {
            expect(martinCostello.londonTravel.Debug.branch()).toBe("master");
        });

        it("then the revision is correct", function () {
            expect(martinCostello.londonTravel.Debug.revision()).toBe("012345ab");
        });
    });

    describe("When logging", function () {

        beforeEach(function () {
            spyOn(console, "log");
        });

        it("then a message is logged", function () {
            martinCostello.londonTravel.Debug.log("2 + 2 = ", 2 + 2);
            expect(console.log).toHaveBeenCalledWith("2 + 2 = ", 4);
        });
    });
});
