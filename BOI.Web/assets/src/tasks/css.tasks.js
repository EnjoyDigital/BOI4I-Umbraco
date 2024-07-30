module.exports = function (gulp, $) {

  //
  // Libsass
  //
  gulp.task('libsass', function () {
    gulp.src(['./assets/dist/css/style.scss', './assets/dist/css/first-paint.scss', './assets/dist/css/wFormOverrides.scss'])
      .pipe($.sourcemaps.init())
      .pipe($.sass().on('error', $.sass.logError))
      .pipe($.autoprefixer({
        browsers: ['last 2 versions'],
        cascade: false
      }))
      .pipe($.sass({ outputStyle: 'compressed' }))
      .pipe($.sourcemaps.write('./'))
      .pipe(gulp.dest('./assets/dist/css'));
  });


  // Umbraco cms rich text editor styles = rte.scss
  gulp.task('rte-libsass', function () {
    gulp.src('./assets/dist/css/rte.scss')
      .pipe($.sourcemaps.init())
      .pipe($.sass().on('error', $.sass.logError))
      .pipe($.autoprefixer({
        browsers: ['last 2 versions'],
        cascade: false
      }))
      .pipe($.sass({ outputStyle: 'compressed' }))
      .pipe($.sourcemaps.write('./'))
      .pipe(gulp.dest('./css'));
  });


  //
  // First Paint
  //
  gulp.task('firstpaint', function () {
    gulp.src('./assets/dist/css/first-paint.css')
      .pipe($.replace('@', '@@'))
      .pipe($.insert.prepend('<style media="screen">'))
      .pipe($.insert.append('</style>'))
      .pipe($.rename({ basename: 'FirstPaint', extname: '.cshtml' }))
      .pipe(gulp.dest('./Views/Shared'));
  });

  //
  // CSS Linter
  //
  gulp.task('sass-lint', function () {
    return gulp.src([
      './assets/dist/css/**/*.scss',
      '!./assets/dist/css/vendor/**/*.scss',
      '!./assets/dist/css/config/*.scss',
      '!./assets/dist/css/maps/*.scss'
    ])
      .pipe($.sassLint({
        options: {
          formatter: 'stylish'
        },
        configFile: '.sasslintrc'
      }))
      .pipe($.sassLint.format())
      .pipe($.sassLint.failOnError())
  });

}