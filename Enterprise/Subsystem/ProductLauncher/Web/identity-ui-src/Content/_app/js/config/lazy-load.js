//  Lazy Load Config

(function (angular) {
    "use strict";

    function config(resolveModule) {
        var modules, appConfig,
            appName = "identity";

        modules = {
            "login.base": ["css", "js", "lang"],
            "user-lookup.base": ["css", "js", "lang"],
            "forgot-password.base": ["css", "js", "lang"],
            "change-password.base": ["css", "js", "lang"]
        };

        appConfig = {
            appName: appName,
            modules: modules,
            basePath: "/login/content"
        };

        resolveModule
            .setLazyLoad(appName, appConfig);
    }

    angular
        .module("identity")
        .config(["rpResolveModuleProvider", config]);
})(angular);
