// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

console.log("Started Service Worker.", self);

self.addEventListener("install", function (event) {
    event.waitUntil(
        caches.open("londontravel.martincostello.com").then(function (cache) {
            return cache.addAll([
                "/",
                "/assets/css/main.css",
                "/assets/js/main.js"
            ]);
        }).then(function () {
            return self.skipWaiting();
        })
    );
    console.log("Installed Service Worker.");
});

self.addEventListener("activate", function (event) {
    console.log("Activated Service Worker.");
});
