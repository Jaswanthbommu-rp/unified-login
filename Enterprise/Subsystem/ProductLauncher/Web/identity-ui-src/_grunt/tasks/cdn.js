module.exports = function (task, env) {
    "use strict";

    task.getDefParams = function (appName) {
        return {
            cdnVer: env.defCdnVer
        };
    };

    task.getTaskConfig = function (appName) {
        var list = [],
            config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "cdn"),
            dest = env.buildPath + params.cdnVer + env.ds + "archive.zip";

        if (env.cdnArchives.indexOf(dest) == -1 && !task.fileExists(dest)) {
            env.cdnArchives.push(dest);

            list.push({
                dest: dest,
                name: "archive",
                src: "/archive.zip"
            });
        }

        list.forEach(function (item) {
            config["cdn.archive." + params.cdnVer] = {
                dest: item.dest,
                src: env.cdn + params.cdnVer + item.src
            };
        });

        return config;
    };

    return task;
};
