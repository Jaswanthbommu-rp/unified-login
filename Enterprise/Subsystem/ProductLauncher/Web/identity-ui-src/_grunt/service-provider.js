module.exports = function (grunt, env) {
    "use strict";

    var prov = {},
        data = {},
        services = {},
        decamelize = require("decamelize");

    prov.getFilePath = function (type, name) {
        var fileName = decamelize(name, "-") + ".js";
        return [type, fileName].join(env.ds);
    };

    prov.getSvc = function (name) {
        if (!services[name]) {
            var filePath = prov.getFilePath("services", name);
            services[name] = env.loadGruntFile(filePath)(grunt, env);
        }

        return services[name];
    };

    prov.getData = function (name) {
        if (!data[name]) {
            var filePath = prov.getFilePath("data", name);
            data[name] = env.loadGruntFile(filePath)(grunt, env);
        }

        return data[name];
    };

    return {
        getSvc: prov.getSvc,
        getData: prov.getData
    };
};
