module.exports = function (grunt, env) {
    "use strict";

    var filters = {},
        fs = require("fs");

    filters.isNew = function (filePath) {
        var srcTime = fs.statSync(filePath).mtime.getTime();
        // don"t watch files changed before last 5 seconds
        return Date.now() - srcTime < 5000;
    };

    return filters;
};
