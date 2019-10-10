//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtDeleteRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/ops/role";

        actions = {
            save: {
                method: "DELETE"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("spndMgmtDeleteRoleSvc", [
            "ENV",
            "$resource",
            SpndMgmtDeleteRoleSvc
        ]);
})(angular);