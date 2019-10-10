module.exports = function (task, env) {
    "use strict";

    task.getOptions = function () {
        return {
            sub: true,
            evil: true,
            curly: true,
            devel: true,
            undef: true,
            white: false,
            nomen: false,
            browser: true,
            latedef: true,
            loopfunc: true,
            smarttabs: true,

            reporter: require("jshint-stylish"),

            globals: {
                $: true,
                it: true,
                logw: true,
                logc: true,
                module: true,
                jQuery: true,
                angular: true,
                require: true,
                OneSite: true,
                jasmine: true,
                RealPage: true,
                Modernizr: true,
                sinon: true,
                expect: true,
                inject: true,
                describe: true,
                beforeEach: true,
                afterEach: true,
                spyOn: true
            }
        };
    };

    task.getSrc = function (appName, task) {
        return task.getSrcList(function (target) {
            if (task.testjs) {
                return [
                    appName + target + "/js*/*.js",
                    appName + target + "/js*/**/*.js",
            		"!" + appName + "/lib/**"
        		];
            }
            else {
                return [
                    appName + target + "/js*/*.js",
                    appName + target + "/js*/**/*.js",
            		"!" + appName + "/lib/**",
                    "!" + appName + target + "/js*/**/*.mock.js",
                    "!" + appName + target + "/js*/**/*.spec.js"
            	];
        	}
        });
    };

    task.getDefParams = function (appName) {
        return {
            getSrc: task.getSrc
        };
    };

    task.getTaskConfig = function (appName) {
        var config = {},
            buFo = task.buildFolder(appName),
            params = task.getParams(appName, "lintjs");

        config.options = task.getOptions();

        config["lintjs." + buFo] = {
            files: [{
                expand: true,
                filter: "isFile",
                src: params.getSrc(appName, task)
            }]
        };

        if (task.watch) {
            config["lintjs." + buFo + ".new"] = {
                files: [{
                    expand: true,
                    filter: task.filters.isNew,
                    src: params.getSrc(appName, task)
                }]
            };
        }

        return config;
    };

    return task;
};
