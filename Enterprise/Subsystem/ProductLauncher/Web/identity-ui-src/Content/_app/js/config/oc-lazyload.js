//  Configure ocLazyLoad

(function (angular) {
    "use strict";

    function config($ocLazyLoadProvider) {
        $ocLazyLoadProvider.config({
            // events: true,
            // debug: true,
            // cache: true
        });
    }

    angular
        .module("identity")
        .config(['$ocLazyLoadProvider', config]);
})(angular);
