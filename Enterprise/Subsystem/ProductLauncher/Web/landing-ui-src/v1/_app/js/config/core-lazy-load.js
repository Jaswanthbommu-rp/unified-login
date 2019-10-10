//  Core Lazy Load Config

(function (angular) {
    "use strict";

    function config(coreLibLazyloadConfig, cdnVer) {
        coreLibLazyloadConfig.init({
            // Customize Base Path
            // basePath: "../"
            basePath: "/home/" + cdnVer
        });
    }

    angular
        .module("settings")
        .config(["coreLibLazyloadConfigProvider", "cdnVer", config]);
})(angular);
