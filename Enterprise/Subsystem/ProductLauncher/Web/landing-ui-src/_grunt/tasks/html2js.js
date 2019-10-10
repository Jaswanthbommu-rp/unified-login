module.exports = function (task, env) {
    "use strict";

    task.getPathPrefix = function (htmlFile) {
        return htmlFile.replace(/(.+)\/temp.+/, "$1");
    };

    task.getKey = function (htmlFile) {
        var key = task.getPathPrefix(htmlFile);
        return key.replace(/\//g, ".").replace(/_/g, "");
    };

    task.getDest = function (htmlFile) {
        var prefix = task.getPathPrefix(htmlFile);
        return prefix + "/js/templates/templates.inc.js";
    };

    task.getModName = function (htmlFile) {
        var prefix = task.getPathPrefix(htmlFile),
            dataFile = prefix + "/templates/mod-name.js",
            modName = task.getKey(htmlFile).split(".")[0];

        if (task.appConfig["html2js"] && task.appConfig["html2js"].modName) {
            modName = task.appConfig["html2js"].modName;
        }

        if (task.fileExists(dataFile)) {
            modName = env.loadFile(dataFile);
        }

        return modName;
    };

    task.genConfig = function (data) {
        return {
            src: data.src,
            dest: data.dest,
            options: {
                indentString: "",
                singleModule: true,
                module: data.modName,
                existingModule: true,
                htmlmin: {
                    collapseWhitespace: true
                },
                rename: function (path) {
                    ["../" + data.appName + "/", /_/g].forEach(function (str) {
                        path = path.replace(str, "");
                    });

                    return path;
                }
            }
        };
    };

    task.getHtmlFiles = function (appName) {
        return task.getSrcList(function (target) {
            return task.expand(appName + target + "/templates/*.html");
        });
    };

    task.getTaskConfig = function (appName) {
        var src = [],
            keys = [],
            config = {},
            htmlFiles = task.getHtmlFiles(appName);

        htmlFiles.forEach(function (htmlFile) {
            var key = task.getKey(htmlFile),
                dest = task.getDest(htmlFile),
                modName = task.getModName(htmlFile),
                src = task.getPathPrefix(htmlFile) + "/templates/*.html";

            if (keys.indexOf(key) == -1) {
                keys.push(key);

                config["html2js." + key] = task.genConfig({
                    src: src,
                    dest: dest,
                    modName: modName,
                    appName: appName
                });
            }
        });

        return config;
    };

    return task;
};
