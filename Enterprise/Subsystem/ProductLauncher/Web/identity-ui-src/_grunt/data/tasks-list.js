module.exports = function (grunt, env) {
    "use strict";

    var list = {};

    list.defaultTask = [
        "psJshint",
        "curl",
        "unzip",
        "copy",
        "psHtml2js",
        "psBundler",
        "htmlmin",
        "sass",
        "string-replace",
        "cssmin",
        "psUglify"
    ];

    list.devTask = [
        "psJshint",
        "curl",
        "unzip",
        "copy",
        "psHtml2js",
        "psBundler",
        "htmlmin",
        "sass",
        "string-replace"
    ];

    if (grunt.option("includeTests")) {
        Object.keys(list).forEach(function (taskName) {
            list[taskName].unshift("concat");
        });
    }

    return {
        "test": [],

        "default": list.defaultTask,

        "dev": list.devTask,

        "formatjs": [
            "jsbeautifier"
        ]
    };
};
