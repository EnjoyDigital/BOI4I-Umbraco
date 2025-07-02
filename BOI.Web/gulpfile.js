//
// Load tasks
//
// Gulp Specific Dependancies
//
const { gulp, src, dest, series, parallel, watch } = require('gulp'),
    webpack = require('webpack-stream'),
    eslint = require('gulp-eslint'),
    sasslint = require('gulp-sass-lint'),
    sourcemaps = require('gulp-sourcemaps'),
    sass = require('gulp-sass'),
    postcss = require('gulp-postcss'),
    replace = require('gulp-replace'),
    insert = require('gulp-insert'),
    rename = require('gulp-rename'),
    autoprefixer = require('autoprefixer'),
    cssnano = require('cssnano'),
    svgSprite = require('gulp-svg-sprite'),
    filter = require('gulp-filter'),

    //
    // File Paths
    //

    //
    // File Paths
    //

    //
    // Bundle the below CSS files
    //
    firstPaintSourcePath = [
        './assets/src/css/first-paint.scss'
    ],
    firstPaintOutputPath = './Views/Shared',
    umbRteSourcePath = [
        './assets/src/css/rte.scss'
    ],
    umbRteOutputPath = './wwwroot/assets/dist/css/rte',
    //
    // first-paint and the RTE styles lint then bundle themselves
    // the main css bundle requires a few exceptions in the watch/lint/bundle paths
    //
    cssWatchPath = [
        './assets/src/css/**/*.scss', //watch all the files
    ],
    cssLintSourcePath = [
        './assets/src/css/**/*.scss', //lint everything
        '!./assets/src/css/vendor/**/*.scss', //except 3rd party plugins
        '!./assets/src/css/maps/*.scss',
        '!./assets/src/css/config/mixins.scss',
    ],
    cssBundleSourcePath = [
        './assets/src/css/*.scss' //bundle anything at the root of /css
    ],
    cssBundleOutputPath = './wwwroot/assets/dist/css',
    webpackConfigPath = './webpack.config', //js bundler config
    //
    // We only run EsLint on the following files.
    // You shouldn't really be adding custom JS anywhere else, but if you do, lint it by adding the path here.
    // Js bundling is handled by WebPack, path to config above.
    //
    jsWatchPath = [
        './assets/src/scripts/**/*.js', //watch all the files
        './assets/src/vue/**/*.{js,vue,json}',
    ],
    // jsLintSourcePath = [
    //     './assets/src/js/**/*.js', //lint most things
    //    '!./assets/src/js/plugins/**/*.js' //excluding 3rd party plugins
    //],
    // JS bundling is handled by WebPack,
    // the bundle source path is in /webpack.config.js
    jsOutputPath = './wwwroot/assets/dist/js',
    webpackVueConfigPath = './webpack.config', //js bundler config


    svgSpriteSourcePath = './assets/src/images/icons/*.svg',
    svgSpriteOutputPath = './Views/Shared',
    //watch view files for updates to auto-reload the page
    razorViewWatchPath = [
        './Views/**/*.cshtml',
        '!./Views/Shared/FirstPaint.cshtml'
    ];

//
// Commands
//
exports.help = series(
    help
)

exports.default = series(
    parallel(
        firstPaint,
        buildSass,
        buildRteSass,
        buildJs,
        buildSvgSprite
    ),
    parallel(
        watchFirstPaint,
        watchSass,
        watchRteSass,
        watchJs,
        watchSvgIcons
    ),
);

exports.devSassOnly = series(
    parallel(
        firstPaint,
        buildSass,
        buildRteSass,
        buildSvgSprite
    ),
    parallel(
        watchFirstPaint,
        watchSass,
        watchRteSass,
        watchSvgIcons
    ),
);

exports.buildOnly = parallel(
    firstPaint,
    buildSass,
    buildRteSass,
    buildJs,
    buildSvgSprite
)

//
// Tasks
//

// Help
function help() {
    return new Promise(function (resolve, reject) { //returning a promise allows the task to be run asyncronously - this is used throughout
        console.log('HELP: What do you need help for? ' + '\n\n');
        console.log('Command: "gulp default" - Builds Everything, then goes on to watch everything. If it\'s not included in the build stop and restart the task' + '\n');
        resolve();
    });
}

//SCSS
function lintSass(pathToLint) { //lint SASS according to rules set in /.sasslintrc
    return new Promise(function (resolve, reject) {
        src(pathToLint)
            .pipe(
                sasslint({
                    options: {
                        formatter: 'stylish'
                    },
                    configFile: '.sasslintrc'
                })
            )
            .pipe(sasslint.format())
            .pipe(sasslint.failOnError())
            .on('error', function (err) {
                console.log(err.toString());
            }
            );
        resolve();
    });
}
function firstPaint() { //converts SASS file to CSS and dumps it into .cshtml file ready for importing into _layout template
    return new Promise(function (resolve, reject) {
        lintSass(firstPaintSourcePath);
        src(firstPaintSourcePath, [allowEmpty = true])
            //build SASS and minify
            .pipe(sourcemaps.init())
            .pipe(sass().on('error', sass.logError))
            .pipe(postcss([autoprefixer(), cssnano()]))
            //convert to .cshtml
            .pipe(replace('@', '@@'))
            .pipe(insert.prepend('<style media="screen">'))
            .pipe(insert.append('</style>'))
            .pipe(rename({ basename: 'FirstPaint', extname: '.cshtml' }))
            .pipe(dest(firstPaintOutputPath));
        resolve();
    });
}

function buildSass() { //build SASS into CSS (bundled via @import rules), run Autoprefixer, minify
    return new Promise(function (resolve, reject) {
        firstPaint();
        lintSass(cssLintSourcePath);
        src(cssBundleSourcePath, [allowEmpty = true])
            .pipe(sourcemaps.init())
            .pipe(sass().on('error', sass.logError))
            .pipe(postcss([autoprefixer(), cssnano()]))
            .pipe(sourcemaps.write('.'))
            .pipe(dest(cssBundleOutputPath));
        resolve();
    });
}

function buildRteSass() { //build SASS for Umbraco RTEs, bundle & prefix, but no minification
    return new Promise(function (resolve, reject) {
        lintSass(umbRteSourcePath);
        src(umbRteSourcePath, [allowEmpty = true])
            .pipe(sourcemaps.init())
            .pipe(sass().on('error', sass.logError))
            .pipe(postcss([autoprefixer()])) //no minification
            .pipe(sourcemaps.write('.'))
            .pipe(dest(umbRteOutputPath));
        resolve();
    });
}

function buildJs() {
    return new Promise(function (resolve, reject) {
        webpack(require(webpackConfigPath)) //do webpack things
            .pipe(dest(jsOutputPath))
            .on('end', function () {
                resolve();
            });
    });
}

//Images
function buildSvgSprite() {
    return new Promise(function (resolve, reject) {
        const svgFilter = filter('**/*.svg', { restore: true });

        src(svgSpriteSourcePath)
            .pipe(
                svgSprite({
                    shape: {
                        dimension: {
                            precision: 2
                        }
                    },
                    mode: {
                        symbol: { // Activate the «symbol» mode
                            dest: '.', // base output folder structure, default adds mode.<mode> folder
                            sprite: './sprite.svg', // sprite output folder structure, default is inside an 'svg' folder
                        }
                    }
                })
            )
            .on('error', function (err) {
                console.log(err.toString());
            }
            )
            .pipe(svgFilter)
            //convert to .cshtml
            .pipe(rename({ basename: 'SvgSprite', extname: '.cshtml' }))
            .pipe(dest(svgSpriteOutputPath))
            .pipe(svgFilter.restore) //probably not needed, but just cleaning up

        resolve();
    });
}

// Watch Tasks
function watchFirstPaint() {
    return new Promise(function (resolve, reject) {
        watch(firstPaintSourcePath, series(
            firstPaint
        ));
        resolve();
    });
}

function watchSass() {
    return new Promise(function (resolve, reject) {
        watch(cssWatchPath, series(
            buildSass
        ));
        resolve();
    });
}

function watchRteSass() {
    return new Promise(function (resolve, reject) {
        watch(umbRteSourcePath, series(
            buildRteSass
        ));
        resolve();
    });
}

function watchJs() {
    return new Promise(function (resolve, reject) {
        watch(jsWatchPath, series(
            // lintJs,
            buildJs
        ));
        resolve();
    });
}

function watchSvgIcons() {
    return new Promise(function (resolve, reject) {
        watch([svgSpriteSourcePath], series(
            buildSvgSprite
        ));
        resolve();
    });
}

