// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(function () {
    if ("serviceWorker" in navigator) {
        navigator.serviceWorker
            .register("/service-worker.js")
            .then(function () {
                console.log("Service Worker registered.");
            })
            .catch(function (e) {
                console.error("Failed to register Service Worker: ", e);
            });
    }
})();
