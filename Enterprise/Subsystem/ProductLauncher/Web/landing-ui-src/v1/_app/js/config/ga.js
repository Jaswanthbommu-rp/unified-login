// Google Analytics

(function (angular) {
    "use strict";

    function setConfig(ga) {
        ga.init("UA-3902027-23");
    }

    angular
        .module("settings")
        .run(["googleAnalytics", setConfig]);
})(angular);
