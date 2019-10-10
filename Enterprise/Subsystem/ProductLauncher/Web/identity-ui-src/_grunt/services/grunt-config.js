module.exports = function (grunt, env) {
    "use strict";

    var svc = {},
        config = {},
        extend = require("extend"),
        tasksList = env.prov.getData("tasksList");

    svc.hasTargets = function (taskName) {
        return Object.keys(config[taskName]).length !== 0;
    };

    svc.setTarget = function (name, value) {
        if (config[name] === undefined) {
            config[name] = {};
        }
        extend(config[name], value);
    };

    svc.init = function () {
        grunt.initConfig(config);
        svc.registerTasks();
    };

    svc.registerTasks = function () {
        Object.keys(tasksList).forEach(function (listName) {
            var list = [];

            tasksList[listName].forEach(function (taskName) {
                if (svc.hasTargets(taskName)) {
                    list.push(taskName);
                }
            });

            grunt.registerTask(listName, list);
        });
    };

    return {
        init: svc.init,
        setTarget: svc.setTarget
    };
};
