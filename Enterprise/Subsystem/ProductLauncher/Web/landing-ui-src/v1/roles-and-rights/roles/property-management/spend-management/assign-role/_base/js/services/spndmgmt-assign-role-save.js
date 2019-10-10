//  New Role Save Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtAssignRoleSaveSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/products/ops/role";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }
    angular
        .module("settings")
        .service("spndMgmtAssignRoleSaveSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            SpndMgmtAssignRoleSaveSvc
        ]);
})(angular);