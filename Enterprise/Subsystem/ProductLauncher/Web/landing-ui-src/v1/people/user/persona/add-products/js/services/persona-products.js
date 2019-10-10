//  User Details Service

(function (angular) {
    "use strict";

    function userSvc($resource, ENV) {
        var svc = {};

        svc.url = {
            products: ENV.landingAPI + "api/organization/:realPageId/products",
            families: ENV.landingAPI + "api/productTypes"
        };

        svc.products = function (orgRealPageId, isMergePersonaAccess) {
            var params = {
                realPageId: orgRealPageId,
                mergePersonaAccess: isMergePersonaAccess
            };

            return $resource(svc.url.products, params);
        };

        svc.families = function () {

            return $resource(svc.url.families);
        };

        return svc;

    }

    angular
        .module("settings")
        .factory("personaProductsSvc", [
          "$resource",
          "ENV",
          userSvc
      ]);
})(angular);
