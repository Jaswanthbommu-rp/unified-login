//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function UserMgmtSetDefaultRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/unifiedlogin/setdefaultrole";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("userMgmtSetDefaultRoleSvc", [
            "ENV",
            "$resource",
            UserMgmtSetDefaultRoleSvc
        ]);
})(angular);