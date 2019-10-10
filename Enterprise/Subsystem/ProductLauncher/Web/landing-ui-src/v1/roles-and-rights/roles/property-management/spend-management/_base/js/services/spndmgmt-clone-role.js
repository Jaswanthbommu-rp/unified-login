//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtCloneRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/ops/role";

        actions = {
            save: {
                method: "POST"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("spndMgmtCloneRoleSvc", [
            "ENV",
            "$resource",
            SpndMgmtCloneRoleSvc
        ]);
})(angular);