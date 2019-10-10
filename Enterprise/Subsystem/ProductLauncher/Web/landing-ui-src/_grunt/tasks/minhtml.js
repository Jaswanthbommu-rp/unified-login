module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/*.html",
                "!" + appName + target + "/templates/*.html"
            ];
        });
    };

    task.getOptions = function () {
        return {
            quoteCharacter: "\"",
            removeComments: true,
            collapseWhitespace: true,
            collapseInlineTagWhitespace: false
        };
    };

    task.getDefParams = function (appName) {
        return {
            src: task.getSrc(appName),

            rename: function (dest, src) {
                return task.cleanPath(env.buildPath + src);
            }
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "minhtml");

        config.options = task.getOptions();

        config["minhtml." + buFo] = {
            files: [{
                expand: true,
                src: params.src,
                filter: "isFile",
                rename: params.rename
            }]
        };

        if (task.watch) {
            config["minhtml." + buFo + ".new"] = {
                files: [{
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
