module.exports = function (grunt, env) {
    "use strict";

    var svc = {},
        baseTask,
        tasks = {},
        path = require("path");

    svc.getFilePath = function (taskName) {
        var filePath = "tasks" + env.ds + taskName + ".js";
        return path.join(env.gruntPath, filePath);
    };

    svc.getBaseTask = function () {
        if (!baseTask) {
            var filePath = svc.getFilePath("base");
            baseTask = require(filePath);
        }

        return baseTask(grunt, env);
    };

    svc.get = function (taskName) {
        if (!tasks[taskName]) {
            var task = svc.getBaseTask(),
                filePath = svc.getFilePath(taskName);
            tasks[taskName] = require(filePath)(task, env);
        }

        return tasks[taskName];
    };

    return {
        get: svc.get
    };
};
