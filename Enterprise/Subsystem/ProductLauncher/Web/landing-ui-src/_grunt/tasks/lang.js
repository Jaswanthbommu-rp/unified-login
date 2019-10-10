module.exports = function (task, env) {
    "use strict";

    task.getOptions = function () {
        return {
            prefix: "{{{ ",
            suffix: " }}}",
            processIncludeContents: function (fileData, local) {
                var filePath = local.includePath.replace(env.basePath, "");
                return "//  Source: " + filePath + "\n" + fileData.trim() + "\n";
            }
        };
    };

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/js-lang/**/scripts.inc"
            ];
        });
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
            params = task.getParams(appName, "lang");

        config.options = task.getOptions();

        config["lang." + buFo] = {
            files: [{
                ext: ".js",
                expand: true,
                src: params.src,
                filter: "isFile",
                rename: params.rename
            }]
        };

        if (task.watch) {
            config["lang." + buFo + ".new"] = {
                files: [{
                    ext: ".js",
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
