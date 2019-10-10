module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/js/**/*.js",
                "!" + appName + "/lib/**"
            ]
        });
    };

    task.getBundleSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/js/scripts.inc"
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

            tasks: [
                "jshint:lintjs." + buFo + ".new",
                "includereplacemore:js." + buFo
            ],

            bundleSrc: task.getBundleSrc(appName),

            bundleTasks: [
                "includereplacemore:js." + buFo + ".new"
            ]
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "watch-js");

        config.options = task.getOptions();

        config["watch-js." + buFo] = {
            files: params.src,
            tasks: params.tasks
        };

        config["watch-js." + buFo + ".bundle"] = {
            files: params.bundleSrc,
            tasks: params.bundleTasks
        };

        return config;
    };

    return task;
};
