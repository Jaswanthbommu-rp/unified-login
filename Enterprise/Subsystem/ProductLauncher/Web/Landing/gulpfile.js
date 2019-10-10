var gulp = require('gulp');
var sass = require('gulp-sass');
var sassLint = require('gulp-sass-lint');
var babel = require('gulp-babel');
var concat = require('gulp-concat');
var uglify = require('gulp-uglify');
var jshint = require('gulp-jshint');
var rename = require('gulp-rename');
var cleanCSS = require('gulp-clean-css');
var util = require('gulp-util');
var del = require('del');
var imagemin = require('gulp-imagemin'),
    cache = require('gulp-cache');

var config = {
    production: !!util.env.production
};

var paths = {
    //Create array of stylesheets
    coreStyles: {
        src: ['Assets/src/sass/variables.scss', 'Assets/src/sass/core.scss', 'Assets/src/sass/custom.scss', 'Assets/src/sass/media-query.scss', 'Assets/src/sass/pages/settings.scss', 'Assets/src/sass/pages/settings-pages.scss', 'Assets/src/sass/pages/system-security.scss', 'Assets/src/sass/pages/helpandsecurity.scss'],
        dest: 'Assets/build/css/'
    },
    //Create array of startup vendor scripts
    vendorScripts: {
        src: [
            'node_modules/foundation-ui-library/jquery/_base/js/jquery.js',
            'node_modules/foundation-ui-library/jquery/tether/js/tether.js',
            'node_modules/foundation-ui-library/bootstrap/_base/js/bootstrap.js',
            'node_modules/foundation-ui-library/js/moment/_base/js/moment.js',
            'node_modules/foundation-ui-library/js/underscore/underscore-min.js',
            'node_modules/foundation-ui-library/jquery/jQuery-Storage-API/jquery.storageapi.min.js',
            'node_modules/foundation-ui-library/jquery/pace/js/pace.js',
            'node_modules/foundation-ui-library/js/moment/_base/js/moment.js',
            'node_modules/foundation-ui-library/jquery/jquery-pjax/jquery.pjax.js',
            'node_modules/jquery-validation/dist/jquery.validate.js'
       ],
        dest: 'Assets/build/js/'
    },
    //Create array of startup custom scripts
    coreScripts: {
        src: [
            'Assets/src/js/config.lazyload.js',
            'Assets/src/js/ui-load.js',
            'Assets/src/js/ui-jp.js',
            'Assets/src/js/ui-include.js',
            'Assets/src/js/ui-json-api-services.js',
            'Assets/src/js/userAuthentication/ui-user-product-selector.js',
            'Assets/src/js/userAuthentication/ui-user-auth-product-icon-selector.js',
            'Assets/src/js/userAuthentication/ui-user-app-picker.js',
            'Assets/src/js/userAuthentication/ui-user-product-card.js',
            'Assets/src/js/userAuthentication/ui-user-auth-data-load.js',
            'Assets/src/js/userAuthentication/ui-user-auth-api-services.js',
            'Assets/src/js/userAuthentication/ui-user-api-service.js',
            'Assets/src/js/ui-reposition.js',
            'Assets/src/js/init-app.js',
           // 'Assets/src/js/raul/nav-header/raul-nav-header.js',
            'Assets/src/js/ui-get-url-variables.js',
            'Assets/src/js/app.js'
        ],
        dest: 'Assets/build/js/'
    }
};

/* Not all tasks need to use streams, a gulpfile is just another node program
 * and you can use all packages available on npm, but it must return either a
 * Promise, a Stream or take a callback and call it
 */
function clean() {
    // You can use multiple globbing patterns as you would with `gulp.src`,
    // for example if you are using del 2.0 or above, return its promise
    return del(['Assets/build']);
}

/*
 * Define our tasks using plain functions
 */
function styles() {
    return gulp.src(paths.coreStyles.src)
        .pipe(sassLint())
        .pipe(sassLint.format())
        .pipe(sassLint.failOnError())
        .pipe(sass())
        .pipe(cleanCSS())
        .pipe(concat('custom.css'))
        .pipe(gulp.dest(paths.coreStyles.dest));
}

//Optimize and transfer all images to the build
function images() {
    return gulp.src('Assets/src/images/**/*')
        .pipe(cache(imagemin([
            imagemin.gifsicle({ interlaced: true }),
            imagemin.jpegtran({ progressive: true }),
            imagemin.optipng({ optimizationLevel: 3 }),
            imagemin.svgo({
                plugins: [
                    { removeViewBox: false }
                ]
            })
        ])))
        .pipe(gulp.dest('Assets/build/images/'));
}

//Process the vendor scripts into a single file and Transfer to build
function vendorScripts() {
    return gulp.src(paths.vendorScripts.src)
        .pipe(config.production ? uglify() : util.noop())
        .pipe(concat('vendor.js'))
        .pipe(gulp.dest(paths.vendorScripts.dest));
}
//Process the custom scripts into a single file and Transfer to build
function coreScripts() {
    return gulp.src(paths.coreScripts.src)
        .pipe(jshint())
        .pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(concat('core.js'))
        .pipe(gulp.dest(paths.coreScripts.dest));
}
//UNCOMMENT for SPA and add spaScripts to gulp.series and gulp build
//Process the scripts in the SPA folder and Transfer to build
/*function spaScripts() {
    return gulp.src('Assets/src/js/spaFramework/*.js')
        //.pipe(jshint())
        //.pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(gulp.dest('Assets/build/js/spaFramework'));
}*/
//Process the scripts in the Company folder and Transfer to build
function companyScripts() {
    return gulp.src('Assets/src/js/company/*.js')
        //.pipe(jshint())
        //.pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(gulp.dest('Assets/build/js/company'));
}
//Process the scripts in the People folder and Transfer to build
function peopleScripts() {
    return gulp.src('Assets/src/js/people/*.js')
        //.pipe(jshint())
        //.pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(gulp.dest('Assets/build/js/people'));
}
//Process the scripts in the Role folder and Transfer to build
function rolesScripts() {
    return gulp.src('Assets/src/js/roles-rights/*.js')
        //.pipe(jshint())
        //.pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(gulp.dest('Assets/build/js/roles-rights'));
}
//Process the scripts in the Settings folder and Transfer to build
function settingsScripts() {
    return gulp.src('Assets/src/js/settings/*.js')
        .pipe(jshint())
        .pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(gulp.dest('Assets/build/js/settings'));
}
//Process the scripts in the base JS folder and Transfer to build
function baseScripts() {
    return gulp.src('Assets/src/js/*.js')
        .pipe(jshint())
        .pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(gulp.dest('Assets/build/js'));
}
//Process the scripts in the SPA folder and Transfer to build
function pluginScripts() {
    return gulp.src('Assets/src/js/plugins/*.js')
        //.pipe(jshint())
        //.pipe(jshint.reporter('default'))
        .pipe(babel({
            'presets': ['es2015']
        }))
        .pipe(config.production ? uglify() : util.noop())
        .pipe(gulp.dest('Assets/build/js/plugins'));
}
//Copy the Foundation UI Library from the Node Modules folder and Transfer to build
function copyUILibrary() {
    return gulp.src('node_modules/foundation-ui-library/**/*')
        .pipe(gulp.dest('Assets/build/foundation-ui-library'));

}
//Copy the Fonts from the Node Modules/Foundation-UI-Library folder and Transfer to build
function copyFonts() {
    return gulp.src('node_modules/foundation-ui-library/fonts/font-awesome/fonts/**/*')
        .pipe(gulp.dest('Assets/build/fonts'))
}
//Watch command available to kick off an automated build when files change.
function watch() {
    gulp.watch('Assets/src/js/**/*.js', gulp.series(coreScripts, companyScripts, peopleScripts, rolesScripts, settingsScripts, baseScripts, pluginScripts));
    gulp.watch(paths.coreStyles.src, styles);
}

/*
 * Specify if tasks run in series or parallel using `gulp.series` and `gulp.parallel`
 */
var build = gulp.series(clean, gulp.parallel(styles, images, vendorScripts, coreScripts, companyScripts, peopleScripts, rolesScripts, settingsScripts, baseScripts, pluginScripts, copyUILibrary, copyFonts));

/*
 * You can still use `gulp.task` to expose tasks
 */
gulp.task('clear-cache', () => cache.clearAll());

/*
 * You can still use `gulp.task` to expose tasks
 */
gulp.task('build', build);

/*
 * Define default task that can be called by just running `gulp` from cli
 */
gulp.task('default', build);