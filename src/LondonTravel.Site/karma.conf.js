// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

module.exports = function (config) {
    config.set({

        autoWatch: false,
        concurrency: Infinity,

        browsers: ["PhantomJS"],
        frameworks: ["jasmine", "karma-typescript"],

        files: [
            "https://ajax.googleapis.com/ajax/libs/jquery/2.2.4/jquery.min.js",
            "assets/scripts/**/*.ts"
        ],

        preprocessors: {
            "**/*.ts": ["karma-typescript"]
        },

        htmlDetailed: {
            splitResults: false
        },

        plugins: [
            "karma-appveyor-reporter",
            "karma-chrome-launcher",
            "karma-html-detailed-reporter",
            "karma-jasmine",
            "karma-phantomjs-launcher",
            "karma-typescript"
        ],

        reporters: [
            "progress",
            "appveyor",
            "karma-typescript"
        ]
    });
};
