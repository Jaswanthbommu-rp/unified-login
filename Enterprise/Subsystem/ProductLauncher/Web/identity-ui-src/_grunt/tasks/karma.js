module.exports = function (task, env) {
    "use strict";

    task.getOptions = function () {
        return {
            files: [
                env.testPath + "lib/app/js-tests/scripts.js",
                env.testPath + "lib/app/js-tests/mocks.js",
                env.testPath + "rp-core/app/js-tests/mocks.js"
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
