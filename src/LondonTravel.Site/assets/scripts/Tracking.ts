// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

export class Tracking {
    static track(category: string, action: string, label: string) {
        let tracked = false;

        if ('gtag' in window) {
            const command = 'event';
            const fields = {
                /* eslint-disable @typescript-eslint/naming-convention */
                event_category: category,
                /* eslint-disable @typescript-eslint/naming-convention */
                event_label: label,
            };

            const theWindow: any = window;
            const gtag = theWindow.gtag || function () {};

            gtag(command, action, fields);

            tracked = true;
        }

        return tracked;
    }
}
