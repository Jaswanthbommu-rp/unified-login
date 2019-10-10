//  New Role Save Service

(function(angular, undefined) {
    "use strict";

    function UserMgmtAssignRoleSaveSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/products/unifiedlogin/role";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);


    }
    angular
        .module("settings")
        .service("userMgmtAssignRoleSaveSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            UserMgmtAssignRoleSaveSvc
        ]);
})(angular);