//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtCloneRoleSaveSvc(ENV, $resource) {
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
        .service("spndMgmtCloneRoleSaveSvc", [
            "ENV",
            "$resource",
            SpndMgmtCloneRoleSaveSvc
        ]);
})(angular);