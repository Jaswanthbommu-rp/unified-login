//  Roles and Rights Assign Products List Service

(function(angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        var params,
            url = ENV.landingAPI + "api/productfamilies";


        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("rolesAndRightsAssignProductsSvc", [
            "$resource",
            "ENV",
            factory
        ]);
})(angular);