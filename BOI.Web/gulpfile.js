//
// Load tasks
//
var gulp = require('gulp'),
    named = require('vinyl-named'),
    webpack = require('webpack-stream');

//
// Load all the plugins into the variable $
//
var $ = require('gulp-load-plugins')({
    pattern: '*'
});

//
// Pass the plugins along so that your tasks can use them
// Will load all xxx.tasks.js files
//
$.loadSubtasks('common/tasks/', $);

//
// Watch
//
gulp.task('watch', function () {
    gulp.watch(['./common/css/**/*.scss'], { verbose: true }, ['libsass', 'firstpaint']);
    gulp.watch(['./common/scripts/pages/*.js', './common/scripts/modules/*.js', './common/scripts/modules/**/*.js', './common/scripts/*.js'], { verbose: true }, ['webpackDev']);
    gulp.watch(['./common/images/icons/*.svg'], { verbose: true }, ['sprite']);
});

//
// Dev webpack build
//
gulp.task('webpackDev', ['js-lint'], function () {
    return gulp.src('./common/scripts/pages/**.js')
        .pipe(named())
        .pipe(webpack(require('./webpack.config.js')))
        .pipe(gulp.dest('./common/scripts/bundles'));
});

//
// Help
//
gulp.task('help', function () {
    console.log('HELP: The following functions can be used currently: ' + '\n\n');
    console.log('Command: "gulp default" - Our normal build. Will build JS stuff, then go onto watching less as usual. Builds with ruby sass.' + '\n');
    console.log('Command: "gulp build" - Build website for production. This includes additional minification of assets.' + '\n');
});

//
// Tasks
//
gulp.task('default', ['firstpaint', 'webpackDev', 'sprite', 'libsass', 'watch']);