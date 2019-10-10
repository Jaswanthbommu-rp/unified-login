module.exports = function (task, env) {
    "use strict";

    var moment = require("moment"),
        appVer = task.gruntOption("appVer") || Date.now().toString(32);

    task.getSrc = function (appName) {
        var buFo = task.buildFolder(appName);

        return task.getSrcList(function (target) {
            return [
                env.buildPath + buFo + "/*.html",
                env.buildPath + buFo + "/startup/js/*.js"
            ];
        });
    };

    task.getDefParams = function (appName) {
        var params = {
            getSrc: task.getSrc,

            rename: function (dest, src) {
                return src;
            },

            replacements: [
                {
                    pattern: /(ENV.APPVER = ")[^"]+(";)/g,
                    replacement: "$1" + appVer + "$2"
                },
                {
                    pattern: /[a-z{}]+("><\/script><\/body>)/ig,
                    replacement: appVer + "$1"
                },
                {
                    pattern: /(ENV.COMPILETIME = ")[^"]+(";)/g,
                    replacement: "$1" + moment().format("MM/DD/YYYY hh:mm:ss a") + "$2"
                },
                {
                    pattern: /(css|js)(: ?")(\.\.\/|\/)?([a-z]+\/)*(.+)(\/lib)/gi,
                    replacement: '$1$2$3$4' + task.getCdnVer() + "$6"
                }
            ]
        };

        if (task.dev) {
            params.replacements.push({
                pattern: /scripts.min.js/g,
                replacement: "scripts.js"
            });
        }

        return params;
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "replace");

        config["stringReplace." + buFo] = {
            options: {
                replacements: params.replacements
            },

            files: [{
                expand: true,
                filter: "isFile",
                rename: params.rename,
                src: params.getSrc(appName, task, env)
            }]
        };

        return config;
    };

    return task;
};
