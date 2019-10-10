// Grunt Config

module.exports = function (grunt) {
    "use strict";

    return {
        skip: [],

        lintjs: {
            getSrc: function (appName, task) {
                return task.getSrcList(function (target) {
                    if (task.testjs) {
                        return [
                            "Content" + target + "/js/*.js",
                            "Content" + target + "/js/**/*.js",
                            "!Content/lib/**"
                        ];
                    }
                    else {
                        return [
                            "Content" + target + "/js/*.js",
                            "Content" + target + "/js/**/*.js",
                            "!Content" + target + "/js/*.min.js",
                            "!Content" + target + "/js/**/*.min.js",
                            "!Content/lib/**"
                        ];
                    }
                });
            }
        },

        replace: {
            getSrc: function (appName, task, env) {
                var buFo = task.buildFolder(appName);

                return task.getSrcList(function (target) {
                    return [
                        env.buildPath + buFo + "/_layout.html",
                        env.buildPath + buFo + "/startup/js/*.js"
                    ];
                });
            }
        },

        html2js: {
            modName: "identity"
        },

        cdn: {
            cdnVer: "v1.10.3"
        }
    };
};
