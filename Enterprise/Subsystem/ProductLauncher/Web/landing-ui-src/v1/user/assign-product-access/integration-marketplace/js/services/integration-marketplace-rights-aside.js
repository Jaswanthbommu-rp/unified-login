//  integMkt Rights List Aside Service

(function(angular) {
    "use strict";

    function IntegMktRightsListAsideSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "/api/products/IntegrationMarketplace/role/rights";
      

        actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("integMktRightsListAsideSvc", ["$resource", "ENV", IntegMktRightsListAsideSvc]);
})(angular);
