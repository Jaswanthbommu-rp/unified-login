//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function UserMgmtDeleteRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/unifiedlogin/role";

        actions = {
            save: {
                method: "DELETE"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("userMgmtDeleteRoleSvc", [
            "ENV",
            "$resource",
            UserMgmtDeleteRoleSvc
        ]);
})(angular);