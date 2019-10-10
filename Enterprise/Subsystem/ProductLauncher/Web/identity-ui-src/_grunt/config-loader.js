module.exports = function (grunt, env) {
    "use strict";

    var svc = {},
        extend = require("extend"),
        taskSvc = env.prov.getSvc("task"),
        appList = env.prov.getSvc("appList"),
        list = env.prov.getData("configList"),
        appConfigSvc = env.prov.getSvc("appConfig"),
        gruntConfig = env.prov.getSvc("gruntConfig");

    svc.setTimer = function () {
        if (grunt.cli.tasks.indexOf("test") == -1) {
            require("time-grunt")(grunt);
        }

        return svc;
    };

    svc.setUsage = function () {
        var usage = env.prov.getSvc("usage");

        grunt.registerTask("usage", function () {
            grunt.log.write(usage.get());
        });
    };

    svc.load = function () {
        svc.setTimer().setUsage();

        Object.keys(list).forEach(function (key) {
            var config = {};

            appList.get().forEach(function (appName) {
                var appConfig = appConfigSvc.get(appName);

                list[key].forEach(function (taskName) {
                    var task = taskSvc.get(taskName);

                    if (appConfig.skip.indexOf(taskName) == -1) {
                        task.setAppConfig(appConfig);
                        extend(config, task.getTaskConfig(appName));
                    }
                });
            });

            gruntConfig.setTarget(key, config);
        });

        gruntConfig.init();
    };

    return {
        load: svc.load
    };
};
