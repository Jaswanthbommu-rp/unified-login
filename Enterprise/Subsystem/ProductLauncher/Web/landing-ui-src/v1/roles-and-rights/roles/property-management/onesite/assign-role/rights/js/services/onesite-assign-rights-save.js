//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function OnesiteAssignRightSavesvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesite/role/rights";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);       
    }

    angular
        .module("settings")
        .service("onesiteAssignRightSavesvc", [
            "ENV",
            "$resource",            
            OnesiteAssignRightSavesvc
        ]);
})(angular);