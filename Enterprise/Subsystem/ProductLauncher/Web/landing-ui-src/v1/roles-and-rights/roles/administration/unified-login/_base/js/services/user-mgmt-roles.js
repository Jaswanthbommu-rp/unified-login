// //  Roles Grid Data Service



(function(angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        var params,
            url = ENV.landingAPI + "api/products/unifiedlogin/rolesCount";


        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("userMgmtRolesSvc", [
            "$resource",
            "ENV",
            factory
        ]);
})(angular);