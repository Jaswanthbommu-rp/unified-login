module.exports = function (task, env) {
    "use strict";

    var initComplete = false;

    task.getTaskConfig = function (appName) {
        var src,
            allowed,
            config = {},
            buildEnv = task.getEnv();

        src = [
            "json/**"
        ];

        allowed = buildEnv == "dev" || buildEnv == "local";

        if (!initComplete && allowed) {
            initComplete = true;

            config["copy.json"] = {
                files: [{
                    src: src,
                    expand: true,
                    cwd: env.basePath,
                    dest: env.buildPath
                }]
            };
        }

        return config;
    };

    return task;
};
