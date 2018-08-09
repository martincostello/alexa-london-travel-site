// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

$(window).on("load", () => {

    const loadImages = (e?: JQuery.Event) => {
        setTimeout(() => {
            ($("img.lazy") as any).lazyload();
        }, 500);
    };

    loadImages();

    $(".navbar-toggle").on("click", loadImages);
});
