// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(function () {
    var trackingId = $("meta[name='google-analytics']").attr("content");
    if (trackingId !== "") {
        (function (i, s, o, g, r, a, m) {
            i.GoogleAnalyticsObject = r; i[r] = i[r] || function () {
                (i[r].q = i[r].q || []).push(arguments);
            }; i[r].l = 1 * new Date(); a = s.createElement(o);
            m = s.getElementsByTagName(o)[0]; a.async = 1; a.src = g; m.parentNode.insertBefore(a, m);
        })(window, document, "script", "//www.google-analytics.com/analytics.js", "ga");
        ga("create", trackingId, "auto");
        ga("send", "pageview");
    }
})();
