module.exports = function (task, env) {
    "use strict";

    task.getPathPrefix = function (htmlFile) {
        return htmlFile.replace(/(.+)\/temp.+/, "$1");
    };

    task.getKey = function (htmlFile) {
        var key = task.getPathPrefix(htmlFile);
        return key.replace(/\//g, ".").replace(/_/g, "");
    };

    task.getHtmlFiles = function (appName) {
        return task.getSrcList(function (target) {
            return task.expand(appName + target + "/templates/*.html");
        });
    };

    task.getOptions = function () {
        return {
            nospawn: true,
            livereload: true,
            events: ["added", "changed"]
        };
    };

    task.getDefParams = function (appName) {
        var buFo = task.buildFolder(appName);

        return {
            src: task.getSrc(appName),
            tasks: ["htmlmin:minhtml." + buFo + ".new"]
        };
    };

    task.getTaskConfig = function (appName) {
        var src = [],
            keys = [],
            config = {},
            buFo = task.buildFolder(appName),
            htmlFiles = task.getHtmlFiles(appName);

        config.options = task.getOptions();

        htmlFiles.forEach(function (htmlFile) {
            var key = task.getKey(htmlFile),
                src = task.getPathPrefix(htmlFile) + "/templates/*.html";

            if (keys.indexOf(key) == -1) {
                keys.push(key);

                config["watch-template." + key] = {
                    files: src,
                    tasks: [
                        "html2js:html2js." + key,
                        "includereplacemore:js." + buFo
                    ]
                };
            }
        });

        return config;
    };

    return task;
};
