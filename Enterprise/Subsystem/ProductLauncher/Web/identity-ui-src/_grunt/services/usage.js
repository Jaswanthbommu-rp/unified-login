module.exports = function (grunt, env) {
    "use strict";

    var message,
        lines = [],
        chalk = require("chalk");

    message = [
        "\n",
        "grunt\n",
        "    Performs full build that includes\n",
        "    minification of CSS and JavaScript bundles\n",

        "grunt dev\n",
        "    Performs full build while skipping the\n",
        "    minification of CSS and JavaScript bundles\n",

        "grunt --appName=lrc\n",
        "    Performs full build for LRC SPA\n",

        "grunt dev --appName=lrc\n",
        "    Performs full build while skipping the\n",
        "    minification of CSS and JavaScript bundles for LRC SPA\n",

        "grunt htmlmin\n",
        "    Performs minification of html views\n",

        "grunt sass\n",
        "    Performs CSS compilation\n",

        "grunt cssmin\n",
        "    Performs minification of CSS bundles\n",

        "grunt html2js\n",
        "    Performs compilation of AngularJS html templates\n",

        "grunt jshint\n",
        "    Performs linting of JavaScript files\n",

        "grunt includereplacemore\n",
        "    Performs compilation of JavaScript bundles\n",

        "grunt uglify\n",
        "    Performs minification of JavaScript bundles\n",

        "grunt copy\n",
        "    Copies image/font files to build folder\n",

        "grunt clean\n",
        "    Deletes cached CDN files\n"
    ];

    message.forEach(function (text, index) {
        if (text.match(/^grunt/)) {
            lines.push(chalk.cyan.bold(text));
        }
        else {
            lines.push(text);
        }
    });

    return {
        get: function () {
            return lines.join("");
        }
    };
};
