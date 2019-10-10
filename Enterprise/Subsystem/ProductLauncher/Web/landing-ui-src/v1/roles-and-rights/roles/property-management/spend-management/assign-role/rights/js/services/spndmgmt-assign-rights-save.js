//  assign Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtAssignRightSavesvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/ops/role/rights";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);       
    }

    angular
        .module("settings")
        .service("spndMgmtAssignRightSavesvc", [
            "ENV",
            "$resource",            
            SpndMgmtAssignRightSavesvc
        ]);
})(angular);