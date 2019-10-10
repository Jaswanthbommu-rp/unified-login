// Location Service Config

(function (angular) {
    "use strict";

    function config($locationProvider) {
        $locationProvider.hashPrefix("");
    }

    angular
        .module("identity")
        .config(["$locationProvider", config]);
})(angular);
