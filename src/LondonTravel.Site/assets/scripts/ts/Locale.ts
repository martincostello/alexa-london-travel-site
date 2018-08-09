// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

declare var moment: any;

(() => {
    const locale: string = $("meta[name='x-request-locale']").attr("content");
    if ("moment" in window && locale) {
        moment.locale(locale);
    }
})();
