module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/css/**/styles.scss",
                "!" + appName + "/lib/**"
            ];
        });
    };

    task.getOptions = function () {
        return {
            noCache: true,
            sourceMap: true,
            style: "expanded",
            require: "sass-css-importer"
        };
    };

    task.getDefParams = function (appName) {
        return {
            rename: function (dest, src) {
                src = env.buildPath + src;
                return task.cleanPath(src);
            },

            src: task.getSrc(appName)
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "css");

        config.options = task.getOptions();

        config["css." + buFo] = {
            files: [{
                ext: ".css",
                expand: true,
                src: params.src,
                filter: "isFile",
                rename: params.rename
            }]
        };

        if (task.watch) {
            config["css." + buFo + ".new"] = {
                files: [{
                    ext: ".css",
                    expand: true,
                    src: params.src,
                    rename: params.rename,
                    filter: task.filters.isNew
                }]
            };
        }

        return config;
    };

    return task;
};
