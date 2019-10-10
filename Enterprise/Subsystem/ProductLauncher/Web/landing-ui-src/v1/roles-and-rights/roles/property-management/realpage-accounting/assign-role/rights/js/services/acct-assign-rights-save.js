//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function AcctAssignRightSavesvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesiteaccounting/role/rights";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);       
    }

    angular
        .module("settings")
        .service("acctAssignRightSavesvc", [
            "ENV",
            "$resource",            
            AcctAssignRightSavesvc
        ]);
})(angular);

