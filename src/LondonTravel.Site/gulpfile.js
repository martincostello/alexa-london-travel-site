// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

/// <binding AfterBuild='build' />

"use strict";

var gulp = require("gulp");
var jasmine = require("gulp-jasmine");
var jshint = require("gulp-jshint");
var karmaServer = require("karma").Server;
var merge = require("merge2");
var path = require("path");
var sourcemaps = require("gulp-sourcemaps");
var ts = require("gulp-typescript");
var tsProject = ts.createProject("tsconfig.json");

var assets = "./assets/";
var scripts = assets + "scripts/";
var jsSrc = scripts + "js/";
var tsSrc = scripts + "ts/";

var paths = {
    js: jsSrc + "**/*.js",
    ts: tsSrc + "**/*.ts",
    testsJs: jsSrc + "**/*.spec.js"
};

gulp.task("build:ts", function () {

    var tsResult = gulp
        .src(paths.ts, { base: tsSrc })
        .pipe(sourcemaps.init({ loadMaps: true }))
        .pipe(tsProject());

    return tsResult.js
        .pipe(sourcemaps.write("."))
        .pipe(gulp.dest(tsSrc));
});

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
