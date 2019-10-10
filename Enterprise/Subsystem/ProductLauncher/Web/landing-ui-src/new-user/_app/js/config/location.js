// Location Service Config

(function (angular) {
    "use strict";

    function config($locationProvider) {
        $locationProvider.hashPrefix("");
    }

    angular
        .module("new-user")
        .config(["$locationProvider", config]);
})(angular);
