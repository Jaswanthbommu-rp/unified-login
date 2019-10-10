module.exports = function (task, env) {
    "use strict";

    task.getSrc = function (appName) {
        return task.getSrcList(function (target) {
            return [
                appName + target + "/js-tests/*.inc"
            ]
        });
    };

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

    task.getDefParams = function (appName) {
        return {
            rename: function (dest, src) {
                src = env.testPath + src;
                return task.cleanTestPath(src);
            },

            src: task.getSrc(appName)
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "testjs");

        config.options = task.getOptions();

        config["testjs." + buFo] = {
            files: [{
                ext: ".js",
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
