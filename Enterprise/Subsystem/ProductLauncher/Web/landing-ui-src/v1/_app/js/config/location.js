// Location Service Config

(function (angular) {
    "use strict";

    function config($locationProvider) {
        $locationProvider.hashPrefix("");
    }

    angular
        .module("settings")
        .config(["$locationProvider", config]);
})(angular);
