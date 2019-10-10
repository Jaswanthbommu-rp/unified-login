module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/*.html",
                "!" + appName + target + "/templates/*.html"
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
            tasks: ["htmlmin:minhtml." + buFo + ".new"]
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "watch-html");

        config.options = task.getOptions();

        config["watch-html." + buFo] = {
            files: params.src,
            tasks: params.tasks
        };

        return config;
    };

    return task;
};
