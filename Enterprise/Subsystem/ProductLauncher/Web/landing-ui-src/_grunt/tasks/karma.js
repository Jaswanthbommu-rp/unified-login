module.exports = function (task, env) {
    "use strict";

    task.getOptions = function () {
        var prefix = env.buildPath + task.getCdnVer() + env.ds + "testing" + env.ds;

        return {
            files: [
                prefix + "base/js/scripts.js",
                prefix + "framework/js/scripts.js",
                prefix + "mocks/js/*.js"
            ]
        };
    };

    task.getDefParams = function (appName) {
        var preprocessors = {},
            buFo = task.buildFolder(appName),
            testPath = env.testPath + buFo + env.ds,
            key = testPath + "**/js-tests/**/scripts.js";

        preprocessors[key] = ["coverage"];

        return {
            files: [{
                expand: true,
                filter: "isFile",
                src: [
                    testPath + "app/js-tests/app.js",
                    testPath + "app/js-tests/mocks.js",
                    testPath + "**/js-tests/scripts.js"
                ]
            }],

            coverageReporter: {
                type: "html",
                dir: env.codeCoverage + buFo + "/"
            },

            preprocessors: preprocessors
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "karma"),
            debug = task.gruntOption("dbg") !== undefined;

        config.options = task.getOptions();

        config["karma." + buFo] = {
            autoWatch: false,
            singleRun: !debug,
            files: params.files,
            configFile: "karma.conf.js",
            coverageReporter: params.coverageReporter,
            browsers: [debug ? "Chrome" : "PhantomJS"],
            preprocessors: debug ? "" : params.preprocessors
        };

        return config;
    };

    return task;
};
