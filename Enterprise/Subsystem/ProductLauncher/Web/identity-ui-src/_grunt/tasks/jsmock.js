module.exports = function (task, env) {
    "use strict";

    task.getDefParams = function (appName) {
        var buFo = task.buildFolder(appName);

        return {
            src: [appName + "/**/js-tests/**/*.mock.js"],
            dest: env.testPath + buFo + "/app/js-tests/mocks.js"
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "jsmock");

        config["jsmock." + buFo] = {
            src: params.src,
            dest: params.dest
        };

        return config;
    };

    return task;
};
