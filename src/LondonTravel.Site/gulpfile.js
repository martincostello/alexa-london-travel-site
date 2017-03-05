// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/// <binding AfterBuild='build' />

"use strict";

var gulp = require("gulp");
var jasmine = require("gulp-jasmine");
var jshint = require("gulp-jshint");
var karmaServer = require("karma").Server;

var webroot = "./wwwroot/assets/";
var assets = "./assets/";
var scripts = assets + "js/";

var paths = {
    js: scripts + "**/*.js",
    testsJs: scripts + "**/*.spec.js"
};

gulp.task("lint:js", function () {
    return gulp.src(paths.js)
        .pipe(jshint())
        .pipe(jshint.reporter("default"))
        .pipe(jshint.reporter("fail"));
});

gulp.task("lint", ["lint:js"]);

gulp.task("test:js:karma", function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        singleRun: true
    }, done).start();
});

gulp.task("test:js:chrome", function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        browsers: ["Chrome"],
        preprocessors: []
    }, done).start();
});

gulp.task("test:js", ["test:js:karma"]);
gulp.task("test", ["test:js"]);

gulp.task("build", ["lint"]);
gulp.task("publish", ["build", "test"]);

gulp.task("default", ["build"]);
