module.exports = function (gulp, $) {

  //
  // JS Linter
  //
  gulp.task('js-lint', function () {
    return gulp.src([
        'common/scripts/modules/**/*.js',
        'common/scripts/pages/*.js'
    ])
    // .pipe($.eslint({
    //   configFile: '.eslintrc'
    // }))
    .pipe($.eslint.format())
    .pipe($.eslint.failAfterError());
  });
}