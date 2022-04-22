// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

describe("Given the namespaces are defined", () => {

    it("then martinCostello is not null", () => {
        expect(martinCostello).not.toBeNull();
    });

    it("then martinCostello.londonTravel is not null", () => {
        expect(martinCostello.londonTravel).not.toBeNull();
    });
});

describe("Google Analytics", () => {

    describe("Given ga is not defined", function () {

        let analytics: any;

        beforeEach(() => {
            analytics = ga;
            ga = null;
        });

        afterEach(() => {
            ga = analytics;
        });

        it("then an event is not published", function () {

            const category = "category";
            const action = "action";
            const label = "label";

            const result = martinCostello.londonTravel.Tracking.track(category, action, label);

            expect(result).toBe(false);
        });
    });

    describe("Given ga is defined", () => {

        beforeEach(() => {
            let spy = jasmine.createSpy("ga");
            ga = (spy as any) as any;
        });

        afterEach(() => {
            ga = null;
        });

        it("then an event is published", () => {

            const category = "category";
            const action = "action";
            const label = "label";

            const result = martinCostello.londonTravel.Tracking.track(category, action, label);

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

describe("Debugging", () => {

    describe("Given meta tags containing the site version", () => {

        beforeAll(() => {
            $("head").append("<meta name='x-site-branch'] content='main' />");
            $("head").append("<meta name='x-site-revision'] content='012345ab' />");
        });

        it("then the branch is correct", () => {
            expect(martinCostello.londonTravel.Debug.branch()).toBe("main");
        });

        it("then the revision is correct", () => {
            expect(martinCostello.londonTravel.Debug.revision()).toBe("012345ab");
        });
    });

    describe("When logging", () => {

        beforeEach(() => {
            spyOn(console, "log");
        });

        it("then a message is logged", () => {
            martinCostello.londonTravel.Debug.log("2 + 2 = ", 2 + 2);
            expect(console.log).toHaveBeenCalledWith("2 + 2 = ", 4);
        });
    });
});
