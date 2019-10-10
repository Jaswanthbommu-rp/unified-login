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
            "clean",
            "clean-cdn"
        ],

        concat: [
            "jsmock"
        ],

        copy: [
            "copy",
            "copy-cdn",
            "copy-env",
            "copy-json"
        ],

        cssmin: [
            "mincss"
        ],

        html2js: [
            "html2js"
        ],

        htmlmin: [
            "minhtml"
        ],

        includereplacemore: [
            "js", "lang"
        ],

        jshint: [
            "lintjs"
        ],

        sass: [
            "css"
        ],

        "string-replace": [
            "replace"
        ],

        uglify: [
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

    if (grunt.cli.tasks.indexOf("testjs") != -1) {
        list.includereplacemore = ["testjs"];
    }

    return list;
};
