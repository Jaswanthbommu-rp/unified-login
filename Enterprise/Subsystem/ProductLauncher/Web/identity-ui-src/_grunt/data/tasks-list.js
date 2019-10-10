module.exports = function (grunt, env) {
    "use strict";

    return {
        "test": [],

        "default": [
            "jshint",
            "curl",
            "unzip",
            "copy",
            "html2js",
            "includereplacemore",
            "htmlmin",
            "sass",
            "string-replace",
            "cssmin",
            "uglify"
        ],

        "dev": [
            "jshint",
            "curl",
            "unzip",
            "copy",
            "html2js",
            "includereplacemore",
            "htmlmin",
            "sass",
            "string-replace"
        ],

        "testjs": [
            "jshint",
            "concat",
            "includereplacemore"
        ],

        "formatjs": [
            "jsbeautifier"
        ]
    };
};
