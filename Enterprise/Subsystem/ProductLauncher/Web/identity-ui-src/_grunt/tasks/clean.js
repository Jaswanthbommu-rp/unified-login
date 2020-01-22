module.exports = function (task, env) {
    "use strict";

    var done = false;

    task.getOptions = function () {
        return {
            force: true
        };
    };

    task.getDefParams = function () {
        return {};
    };

    task.getTaskConfig = function (appName) {
        var src = [],
            config = {},
            options = task.getOptions(),
            buFo = task.buildFolder(appName);

        if (!done) {
            done = true;
            src = [env.basePath + '.grunt-cache'];

            if (env.activeApp == "*") {
                src.push(env.buildPath + "v{1,2}*");
            }
            else {
                src.push(env.buildPath + task.getCdnVer());
            }

            config["clean"] = {
                src: src,
                options: options
            };
        }

        src = [
            env.buildPath + buFo,
            env.basePath + appName + env.ds + "lib"
        ];

        config["clean." + buFo] = {
            src: src,
            options: options
        };

        return config;
    };

    return task;
};
