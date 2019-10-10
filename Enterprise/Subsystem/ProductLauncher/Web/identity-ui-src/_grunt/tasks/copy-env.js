module.exports = function (task, env) {
    "use strict";

    var logged = false;

    task.getSrc = function (appName) {
        var env = task.getEnv(appName);

        return [
            appName + "/**/environment." + env
        ];
    };

    task.getDefParams = function (appName) {
        var env = task.getEnv();

        if (!logged) {
            logged = true;
            task.log("env provided: " + task.gruntOption("env"));
            task.log("env used: " + env);
        }

        return {
            src: task.getSrc(appName),

            rename: function (dest, src) {
                return src.replace(env, "js");
            }
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "copyEnv");

        config["copy." + buFo + ".env"] = {
            files: [{
                expand: true,
                src: params.src,
                filter: "isFile",
                rename: params.rename
            }]
        };

        return config;
    };

    return task;
};
