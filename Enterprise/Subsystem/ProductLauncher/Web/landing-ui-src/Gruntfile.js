//  Grunt Master

module.exports = function (grunt) {
    "use strict";

    var path = require("path"),
        ds = path.sep,
        basePath = path.resolve(".") + ds;

    require("load-grunt-tasks")(grunt, {
        scope: "devDependencies"
    });

    var env = {
        ds: ds,
        cdnArchives: [],
        basePath: basePath,
        defCdnVer: "v1.5.0",
        gruntPath: basePath + "_grunt" + ds,
        buildPath: path.resolve("../landing") + ds,
        activeApp: grunt.option("appName") || "*",
        testPath: path.resolve("../ui-tests") + ds,
        codeCoverage: path.resolve("../code-coverage") + ds,
        cdn: "http://uicdn.internaldev.realpage.com/"
    };

    env.loadFile = function (filePath) {
        filePath = path.join(env.basePath, filePath);
        return require(filePath);
    };

    env.loadGruntFile = function (filePath) {
        filePath = path.join(env.gruntPath, filePath);
        return require(filePath);
    };

    env.prov = env.loadGruntFile("service-provider.js")(grunt, env);

    env.loadGruntFile("config-loader.js")(grunt, env).load();
};
