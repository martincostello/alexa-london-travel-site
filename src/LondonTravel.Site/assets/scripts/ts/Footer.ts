// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

declare var moment: any;

(() => {
    $(document).ready((): void => {

        let element: JQuery = $("#build-date");

        if (element && "moment" in window) {

            let timestamp: string = element.attr("data-timestamp");
            let format: string = element.attr("data-format");

            let value: any = moment(timestamp, format);

            if (value.isValid()) {
                let text: string = value.fromNow();
                element.text(`| Last updated ${text}`);
            }
        }
    });
})();
