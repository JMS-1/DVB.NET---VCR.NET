/// <binding BeforeBuild='sass' />
"use strict";

var gulp = require("gulp");
var sass = require("gulp-sass");

gulp.task('sass', function () {
  gulp.src("Content/styles.scss")
      .pipe(sass().on('error', sass.logError))
      .pipe(gulp.dest("Content"));
});

gulp.task('sass:watch', function () {
  gulp.watch(["content/**/*.scss", "src/**/*.scss"], ['sass']);
});
