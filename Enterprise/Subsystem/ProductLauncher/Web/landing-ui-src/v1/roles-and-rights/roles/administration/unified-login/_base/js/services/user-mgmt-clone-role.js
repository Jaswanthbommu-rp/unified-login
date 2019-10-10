//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function UserMgmtCloneRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/unifiedlogin/role";

        actions = {
            save: {
                method: "POST"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("userMgmtCloneRoleSvc", [
            "ENV",
            "$resource",
            UserMgmtCloneRoleSvc
        ]);
})(angular);