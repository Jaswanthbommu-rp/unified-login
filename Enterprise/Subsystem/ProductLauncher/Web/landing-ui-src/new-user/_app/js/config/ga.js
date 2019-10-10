// Google Analytics

(function (angular) {
    "use strict";

    function setConfig(ga) {
        ga.init("GTM-5JP3GV");
    }

    angular
        .module("new-user")
        .run(["googleAnalytics", setConfig]);
})(angular);
