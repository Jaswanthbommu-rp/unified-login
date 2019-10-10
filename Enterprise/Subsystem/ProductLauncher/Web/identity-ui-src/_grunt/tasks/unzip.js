module.exports = function (task, env) {
    "use strict";

    var done = false;

    task.getOptions = function () {
        return {};
    };

    task.getDefParams = function (appName) {
        return {};
    };

    task.getTaskConfig = function (appName) {
        var config = {};

        if (!done) {
            var files = env.cdnArchives;

            files.forEach(function (file) {
                var cdnVer = task.getVer(file);

                config["unzip." + cdnVer] = {
                    src: file,
                    dest: env.buildPath + cdnVer + env.ds
                };
            });

            done = true;
        }

        return config;
    };

    task.getVer = function (file) {
        return file.replace(/.*(v[\d\.]+).*/g, "$1");
    };

    return task;
};
