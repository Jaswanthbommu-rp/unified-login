//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtNewSaveRightsSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/ops/role/rights";

        actions = {
            save: {
                method: "POST"
            }
        };

        return $resource(url, {}, actions);

    }

    angular
        .module("settings")
        .service("spndMgmtNewSaveRightsSvc", [
            "ENV",
            "$resource",
            SpndMgmtNewSaveRightsSvc
        ]);
})(angular);