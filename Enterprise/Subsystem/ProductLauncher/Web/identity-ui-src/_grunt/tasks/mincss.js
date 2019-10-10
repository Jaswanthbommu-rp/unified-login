module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        var buFo = task.buildFolder(appName);

        return task.getSrcList(function (target) {
            return [
                env.buildPath + buFo + target + "/css/**/styles.css"
            ];
        });
    };

    task.getDefParams = function (appName) {
        return {
            src: task.getSrc(appName)
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "mincss");

        config["mincss." + buFo] = {
            files: [{
                expand: true,
                src: params.src,
                ext: ".min.css",
                filter: "isFile"
            }]
        };

        return config;
    };

    return task;
};
