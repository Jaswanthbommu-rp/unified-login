module.exports = function (config) {
    "use strict";

    config.set({
        frameworks: ["jasmine"],

        port: 9876,

        runnerPort: 9100,

        colors: true,

        logLevel: config.LOG_INFO,

        autoWatch: false,

        //  Case sensitive!!
        //  Chrome, ChromeCanary, Firefox, Opera, Safari, PhantomJS, IE

        browsers: ["PhantomJS"],

        captureTimeout: 60000,

        singleRun: true,

        plugins: [
            "karma-jasmine",
            "karma-coverage",
            "karma-ie-launcher",
            "karma-spec-reporter",
            "karma-safari-launcher",
            "karma-chrome-launcher",
            "karma-firefox-launcher",
            "karma-phantomjs-launcher"
        ],

        reporters: ["spec", "coverage"],

        coverageReporter: {
            type: "html",
            dir: "code-coverage/"
        },

        preprocessors: {
            "ui-tests/**/js-tests/**/scripts.js": ["coverage"]
        },

        customLaunchers: {
            IE9: {
                base: "IE",
                "x-ua-compatible": "IE=EmulateIE9"
            },

            IE8: {
                base: "IE",
                "x-ua-compatible": "IE=EmulateIE8"
            }
        }
    });
};
