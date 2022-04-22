// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(() => {
    const trackingId: string = $("meta[name='google-analytics']").attr("content");
    if (trackingId !== "") {
        const window: any = Window;
        window.dataLayer = window.dataLayer || [];
        const gtag = (...args: any[]) => window.dataLayer.push(args);
        gtag("js", new Date());
        gtag("config", trackingId);
    }
})();
