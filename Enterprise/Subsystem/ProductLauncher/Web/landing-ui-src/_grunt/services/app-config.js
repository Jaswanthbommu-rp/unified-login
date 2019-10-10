module.exports = function (grunt, env) {
    "use strict";

    var svc = {},
        config = {},
        path = require("path");

    svc.getFilePath = function (appName) {
        var parts = [appName, ".build-config", "config.js"];
        return path.join(env.basePath, parts.join(env.ds));
    };

    svc.get = function (appName) {
        if (!config[appName]) {
            var filePath = svc.getFilePath(appName);
            config[appName] = require(filePath)(grunt, env);
        }

        return config[appName];
    };

    return {
        get: svc.get
    };
};
