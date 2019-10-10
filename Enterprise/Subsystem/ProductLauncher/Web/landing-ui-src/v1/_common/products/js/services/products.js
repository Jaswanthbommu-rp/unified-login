//  Products Data Service

(function (angular, undefined) {
    "use strict";

    function factory($resource, ENV) {
        var params,
            url = ENV.landingAPI + "api/organization/:realPageId/products";

        params = {
            realPageId: "@realPageId"
        };

        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("productsDataSvc", [
            "$resource",
            "ENV",
            factory
        ]);
})(angular);
