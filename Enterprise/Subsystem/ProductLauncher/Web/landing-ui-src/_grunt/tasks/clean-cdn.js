module.exports = function (task, env) {
    "use strict";

    var done = false;

    task.getOptions = function () {
        return {
            force: true
        };
    };

    task.getDefParams = function (appName) {
        return {};
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            options = task.getOptions(),
            appSet = env.activeApp != "*",
            cdnPath = appSet ? task.getCdnVer() : "v{1,2}*";

        if (!done) {
            done = true;

            config["clean.cdn"] = {
                options: options,
                src: env.buildPath + cdnPath
            };
        }

        return config;
    };

    return task;
};
