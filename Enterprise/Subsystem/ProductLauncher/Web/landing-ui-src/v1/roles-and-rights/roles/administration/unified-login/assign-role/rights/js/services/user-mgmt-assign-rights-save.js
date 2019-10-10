//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function UserMgmtAssignRightSavesvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/unifiedlogin/role/rights";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);

    }

    angular
        .module("settings")
        .service("userMgmtAssignRightSavesvc", [
            "ENV",
            "$resource",
            UserMgmtAssignRightSavesvc
        ]);
})(angular);