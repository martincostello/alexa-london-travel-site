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
