module.exports = function (task, env) {
    "use strict";

    task.getOptions = function () {
        return {
            js: {
                brace_style: "end-expand",
                break_chained_methods: false,
                e4x: false,
                end_with_newline: true,
                indentChar: " ",
                indentLevel: 0,
                indentSize: 4,
                indentWithTabs: false,
                jslintHappy: true,
                keepArrayIndentation: true,
                keepFunctionIndentation: false,
                maxPreserveNewlines: 2,
                preserveNewlines: true,
                spaceAfterAnonFunction: true,
                spaceBeforeConditional: true,
                spaceInEmptyParen: false,
                spaceInParen: false,
                unescapeStrings: false,
                wrapLineLength: 0
            }
        };
    };

    task.getDefParams = function (appName) {
        return {
            src: [
                appName + "/**/js/**/*.js",
                appName + "/**/js-tests/**/*.js",
                appName + "/**/js-lang/**/*.js",
                "!" + appName + "/**/templates/**/*.js",
                "!" + appName + "/lib/**"
            ]
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "formatjs");

        config.options = task.getOptions();

        config["formatjs." + buFo] = {
            files: [{
                expand: true,
                src: params.src,
                filter: "isFile"
            }]
        };

        return config;
    };

    return task;
};
