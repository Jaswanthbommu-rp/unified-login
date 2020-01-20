module.exports = function (grunt, env) {
    "use strict";

    var list = {
        curl: [
            "cdn"
        ],

        unzip: [
            "unzip"
        ],

        clean: [
            "clean"
        ],

        concat: [
            "jsmock"
        ],

        copy: [
            "copy",
            "copy-cdn",
            "copy-env"
        ],

        cssmin: [
            "mincss"
        ],

        psHtml2js: [
            "html2js"
        ],

        htmlmin: [
            "minhtml"
        ],

        psBundler: [
            "js",
            "lang"
        ],

        psJshint: [
            "lintjs"
        ],

        sass: [
            "css"
        ],

        "string-replace": [
            "replace"
        ],

        psUglify: [
            "minjs",
            "minlang"
        ],

        karma: [
            "karma"
        ],

        jsbeautifier: [
            "formatjs"
        ]
    };

    if (grunt.cli.tasks.indexOf("watch") != -1) {
        list["watch"] = [
            "watch-css",
            "watch-html",
            "watch-js",
            "watch-lang",
            "watch-template"
        ];
    }

    if (grunt.option("includeTests")) {
        list.psBundler.push("testjs");
    }

    return list;
};
