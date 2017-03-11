// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(function () {
    var locale = $("meta[name='x-request-locale']").attr("content");
    if (moment && locale) {
        moment.locale(locale);
    }
})();
