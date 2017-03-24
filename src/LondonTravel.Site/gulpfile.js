/// <binding Clean='clean' ProjectOpened='watch' />

// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var gulp = require("gulp");
var bundleconfig = require("./bundleconfig.json");
var concat = require("gulp-concat");
var cssmin = require("gulp-cssmin");
var del = require("del");
var jasmine = require("gulp-jasmine");
var jshint = require("gulp-jshint");
var karmaServer = require("karma").Server;
var merge = require("merge-stream");
var path = require("path");
var sourcemaps = require("gulp-sourcemaps");
var ts = require("gulp-typescript");
var tslint = require("gulp-tslint");
var uglify = require("gulp-uglify");

var assets = "./assets/";
var scripts = assets + "scripts/";
var jsSrc = scripts + "js/";
var tsSrc = scripts + "ts/";

var paths = {
    js: jsSrc + "**/*.js",
    ts: tsSrc + "**/*.ts",
    testsJs: jsSrc + "**/*.spec.js"
};

var regex = {
    css: /\.css$/,
    js: /\.js$/
};

function getBundles(regexPattern) {
    return bundleconfig.filter(function (bundle) {
        return regexPattern.test(bundle.outputFileName);
    });
}

gulp.task("clean", function () {
    var files = bundleconfig.map(function (bundle) {
        return bundle.outputFileName;
    });
    return del(files);
});

gulp.task("lint:js", function () {
    return gulp.src(paths.js)
        .pipe(jshint())
        .pipe(jshint.reporter("default"))
        .pipe(jshint.reporter("fail"));
});

gulp.task("lint:ts", function () {
    gulp.src(paths.ts)
        .pipe(tslint({
            formatter: "msbuild"
        }))
        .pipe(tslint.report());
});

gulp.task("lint", ["lint:js", "lint:ts"]);

gulp.task("min:css", function () {
    var tasks = getBundles(regex.css).map(function (bundle) {
        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(cssmin())
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min:js", ["lint:js", "lint:ts"], function () {
    var tasks = getBundles(regex.js).map(function (bundle) {

        var tsProject = ts.createProject("tsconfig.json");
        var tsResult = gulp.src(bundle.inputFiles, { base: "." })
            .pipe(sourcemaps.init())
            .pipe(tsProject());

        return tsResult.js
            .pipe(concat(bundle.outputFileName))
            .pipe(uglify())
            .pipe(sourcemaps.write())
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min", ["min:css", "min:js"]);

gulp.task("test:js:karma", ["min:js"], function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        singleRun: true
    }, done).start();
});

gulp.task("test:js:chrome", ["min:js"], function (done) {
    new karmaServer({
        configFile: __dirname + "/karma.conf.js",
        browsers: ["Chrome"],
        preprocessors: []
    }, done).start();
});

gulp.task("test", ["test:js"]);
gulp.task("test:js", ["test:js:karma"]);

gulp.task("watch", function () {
    getBundles(regex.js).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ["min:js"]);
    });
    getBundles(regex.css).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ["min:css"]);
    });
});

gulp.task("build", ["lint", "min"]);
gulp.task("publish", ["build", "test"]);

gulp.task("default", ["build"]);
