module.exports = function (task, env) {
    "use strict";

    task.getCdnVer = function () {
        var hasConfig = task.appConfig.cdn && task.appConfig.cdn.cdnVer;
        return hasConfig ? task.appConfig.cdn.cdnVer : env.defCdnVer;
    };

    task.getDefParams = function (appName) {
        var cdnVer = task.getCdnVer();

        return {
            src: ["**", "!*.zip"],
            cwd: env.buildPath + cdnVer + env.ds,
            dest: env.basePath + appName + env.ds
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "copy"),
            libPath = params.dest + "lib" + env.ds;

        if (!task.dirExists(libPath)) {
            config["copy." + buFo + ".cdn"] = {
                files: [{
                    expand: true,
                    cwd: params.cwd,
                    src: params.src,
                    dest: params.dest
                }]
            };
        }

        return config;
    };

    return task;
};
