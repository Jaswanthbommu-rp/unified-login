//  Core Lazy Load Config

(function (angular) {
    "use strict";

    function config(coreLibLazyloadConfig) {
        coreLibLazyloadConfig.init({
            // Customize Base Path
            // basePath: "../"
        });
    }

    angular
        .module("identity")
        .config(["coreLibLazyloadConfigProvider", config]);
})(angular);
