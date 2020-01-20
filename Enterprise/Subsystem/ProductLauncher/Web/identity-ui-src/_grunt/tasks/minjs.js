module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        var buFo = task.buildFolder(appName);

        return task.getSrcList(function (target) {
            return [
                env.buildPath + buFo + target + "/js/**/scripts.js"
            ]
        });
    };

    task.getOptions = function () {
        return {
            drop_console: true,

            compress: {
                pure_funcs: ["logc", "logw", "console.log"]
            }
        };
    };

    task.getDefParams = function (appName) {
        return {
            src: task.getSrc(appName)
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "minjs");

        config.options = task.getOptions();

        config["minjs." + buFo] = {
            files: [{
                expand: true,
                ext: ".min.js",
                src: params.src,
                filter: "isFile"
            }]
        };

        return config;
    };

    return task;
};
