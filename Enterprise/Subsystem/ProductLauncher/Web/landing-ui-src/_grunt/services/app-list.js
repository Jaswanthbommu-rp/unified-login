module.exports = function (grunt, env) {
    "use strict";

    var svc = {},
        appList = [],
        path = require("path");

    svc.get = function () {
        if (appList.length === 0) {
            appList = svc.getList();
        }

        return appList;
    };

    svc.getList = function () {
        if (grunt.option("appName") !== undefined) {
            return [grunt.option("appName")];
        }

        var list = [],
            patt = "*/.build-config/config.js",
            pathExp = path.join(env.basePath, patt),
            filesList = grunt.file.expand(pathExp);

        filesList.forEach(function (filePath) {
            list.push(filePath.replace(/.*\/([^\/]+)\/\.build\-config\/.*/, "$1"));
        });

        return list;
    };

    return {
        get: svc.get
    };
};
