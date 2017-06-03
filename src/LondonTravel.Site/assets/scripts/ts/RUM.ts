// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

(() => {
    let rumId: string = $("meta[name='pingdom-rum']").attr("content");

    if (rumId !== "") {

        (window as any)._prum = [
            ["id", rumId],
            ["mark", "firstbyte", (new Date()).getTime()]
        ];

        (() => {
            let s = document.getElementsByTagName("script")[0];
            let p: any = document.createElement("script");
            p.async = "async";
            p.src = "https://rum-static.pingdom.net/prum.min.js";
            s.parentNode.insertBefore(p, s);
        })();
    }
})();
