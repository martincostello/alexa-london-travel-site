// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

export class Telemetry {
    initialize() {
        const element = document.querySelector('meta[name="x-site-user-id"]');
        if (element) {
            const userId = element.getAttribute('content');
            if (userId) {
                let client = (window as any).appInsights as Microsoft.ApplicationInsights.IAppInsights;
                if (client) {
                    client.setAuthenticatedUserContext(userId);
                }
            }
        }
    }
}
