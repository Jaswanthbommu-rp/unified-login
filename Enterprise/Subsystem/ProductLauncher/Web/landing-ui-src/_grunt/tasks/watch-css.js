module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/css/**/*.scss",
                "!" + appName + target + "/css/**/styles.scss"
            ]
        });
    };

    task.getBundleSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/css/**/styles.scss"
            ]
        });
    };

    task.getOptions = function () {
        return {
            nospawn: true,
            livereload: true,
            events: ["added", "changed"]
        };
    };

    task.getDefParams = function (appName) {
        var buFo = task.buildFolder(appName);

        return {
            src: task.getSrc(appName),

            tasks: ["sass:css." + buFo],

            bundleSrc: task.getBundleSrc(appName),

            bundleTasks: ["sass:css." + buFo + ".new"]
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "watch-css");

        config.options = task.getOptions();

        config["watch-css." + buFo] = {
            files: params.src,
            tasks: params.tasks
        };

        config["watch-css." + buFo + ".bundle"] = {
            files: params.bundleSrc,
            tasks: params.bundleTasks
        };

        return config;
    };

    return task;
};
