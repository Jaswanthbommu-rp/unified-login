//  Lazy Load Config

(function (angular) {
    "use strict";

    function config(resolveModule) {
        var modules, appConfig,
            appName = "new-user";

        modules = {
            "error": ["css", "js", "lang"],
            "security-questions": ["css", "js", "lang"],
            "set-password.base": ["css", "js", "lang"],
            "start-profile": ["css", "js", "lang"],
            "validate": ["css", "js", "lang"],
            "validate-token": ["css", "js", "lang"]
        };

        appConfig = {
            appName: appName,
            modules: modules
        };

        resolveModule
            .setLazyLoad(appName, appConfig);
    }

    angular
        .module("new-user")
        .config(["rpResolveModuleProvider", config]);
})(angular);
