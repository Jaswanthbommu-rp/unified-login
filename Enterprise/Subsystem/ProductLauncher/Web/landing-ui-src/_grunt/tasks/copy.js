module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/fonts/*.*",
                appName + target + "/images/*.*",
                appName + target + "/mocks/*.*",
                "!" + appName + "/lib/**"
            ];
        });
    };

    task.getDefParams = function (appName) {
        return {
            src: task.getSrc(appName),

            rename: function (dest, src) {
                var path = src.replace(/(.+)\/[a-z_0-9]+(\.[a-z_0-9]+)+$/i, "$1");
                return env.buildPath + src.replace(path, path.replace(/_/g, ""));
            }
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "copy");

        config["copy." + buFo] = {
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
