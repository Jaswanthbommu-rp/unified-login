//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function OnesiteNewSaveRightsSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesite/role/rights";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);

    }

    angular
        .module("settings")
        .service("onesiteNewSaveRightsSvc", [
            "ENV",
            "$resource",
            OnesiteNewSaveRightsSvc
        ]);
})(angular);