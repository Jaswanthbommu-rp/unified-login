// Users Filter Service

(function (angular) {
    "use strict";

    function usersFilterSvc($resource, ENV) {

        var svc = {};

        svc.getOrganizationProducts = function (params) {

            var actions = {
                get: {
                    method: "GET",
                    params: params
                }
            };

            return $resource(ENV.landingAPI + "/api/organization/:realPageId/products", {}, actions).get().$promise;

        };

        svc.getProperties = function () {
            //TODO update URL when service is available - NOT MVP
            //$resource(ENV.landingAPI + "api/users/property-list");
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("usersFilterSvc", [
            "$resource",
            "ENV",
            usersFilterSvc
        ]);
})(angular);
