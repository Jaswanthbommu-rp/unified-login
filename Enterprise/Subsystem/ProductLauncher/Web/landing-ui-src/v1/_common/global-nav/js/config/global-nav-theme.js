//  Init Global Nav Theme Svc

(function (angular) {
    "use strict";

    function config(svc) {
        svc.init();
    }

    angular
        .module("settings")
        .run(["globalNavThemeSvc", config]);
})(angular);
