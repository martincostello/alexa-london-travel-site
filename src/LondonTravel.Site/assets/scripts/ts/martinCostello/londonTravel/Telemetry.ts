// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace martinCostello.londonTravel {

    /**
     * Represents the class for application telemetry.
     */
    export class Telemetry {

        private readonly client: Microsoft.ApplicationInsights.IAppInsights;

        /**
         * Initializes a new instance of the martinCostello.londonTravel.Telemetry class.
         * @param client - The Application Insights client to use.
         * @constructor
         */
        public constructor(client: Microsoft.ApplicationInsights.IAppInsights) {
            this.client = client;
        }

        /**
         * Initializes telemetry collection.
         */
        public initialize(): void {

            const userId = $("meta[name='x-site-user-id']").attr("content");

            if (userId) {
                this.client.setAuthenticatedUserContext(userId);
            }
        }
    }
}

(() => {
    let client = ((window as any).appInsights as Microsoft.ApplicationInsights.IAppInsights);

    if (client !== undefined) {
        const telemetry = new martinCostello.londonTravel.Telemetry(client);
        telemetry.initialize();
    }
})();
