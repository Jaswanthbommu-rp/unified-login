//  Assign Products List Service

(function (angular) {
    "use strict";

    function assignProductsSvc($resource, ENV) {
        var url, defaults, actions;

        actions = {};

        defaults = {};

        url = ENV.landingAPI + "api/productfamilies";

        return $resource(url, defaults, actions);
    }

    angular
        .module("settings")
        .factory("assignProductsSvc", ["$resource", "ENV", assignProductsSvc]);
})(angular);
