/// <binding Clean='clean' ProjectOpened='publish, watch' />
// Copyright (c) Martin Costello, 2017. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

"use strict";

var gulp = require("gulp");
var bundleconfig = require("./bundleconfig.json");
var concat = require("gulp-concat");
var csslint = require("gulp-csslint");
var cssmin = require("gulp-cssmin");
var del = require("del");
var jshint = require("gulp-jshint");
var karmaServer = require("karma").Server;
var merge = require("merge-stream");
var sourcemaps = require("gulp-sourcemaps");
var ts = require("gulp-typescript");
var tslint = require("gulp-tslint");
var uglify = require("gulp-uglify");

var assets = "./assets/";
var scripts = assets + "scripts/";
var jsSrc = scripts + "js/";
var tsSrc = scripts + "ts/";
var styles = assets + "styles/";
var cssSrc = styles + "css/";

var paths = {
    css: cssSrc + "**/*.css",
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

gulp.task("lint:css", function () {
    return gulp.src(paths.css)
        .pipe(csslint())
        .pipe(csslint.formatter())
        .pipe(csslint.formatter("fail"));
});

gulp.task("lint:js", function () {
    return gulp.src(paths.js)
        .pipe(jshint())
        .pipe(jshint.reporter("default"))
        .pipe(jshint.reporter("fail"));
});

gulp.task("lint:ts", function () {
    return gulp.src(paths.ts)
        .pipe(tslint({
            formatter: "msbuild"
        }))
        .pipe(tslint.report());
});

gulp.task("lint", gulp.parallel("lint:css", "lint:js", "lint:ts"));

gulp.task("min:css", function () {
    var tasks = getBundles(regex.css).map(function (bundle) {

        var css = gulp.src(bundle.inputFiles, { base: "." })
            .pipe(sourcemaps.init())
            .pipe(concat(bundle.outputFileName));

        if (bundle.minify.enabled === true) {
            css = css.pipe(cssmin());
        }

        return css
            .pipe(sourcemaps.write("."))
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min:js", function () {
    var tasks = getBundles(regex.js).map(function (bundle) {

        var tsProject = ts.createProject("tsconfig.json");
        var tsResult = gulp.src(bundle.inputFiles, { allowEmpty: true, base: "." })
            .pipe(sourcemaps.init())
            .pipe(tsProject());

        var compiled = tsResult.js
            .pipe(concat(bundle.outputFileName));

        if (bundle.minify.enabled === true) {
            compiled = compiled.pipe(uglify());
        }

        return compiled.pipe(sourcemaps.write("."))
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min", gulp.parallel("min:css", "min:js"));

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

gulp.task("test:js", gulp.series("test:js:karma"));
gulp.task("test", gulp.series("test:js"));

gulp.task("watch", function () {
    getBundles(regex.css).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, gulp.series("min:css"));
    });
    getBundles(regex.js).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, gulp.series("min:js"));
    });
});

gulp.task("build", gulp.series("lint", "min"));
gulp.task("publish", gulp.series("build", "test"));

gulp.task("default", gulp.series("build"));
