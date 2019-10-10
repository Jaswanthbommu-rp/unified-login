//  Clone Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function OnesiteCloneRightsSaveSvc(ENV, $resource) {
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
        .service("onesiteCloneRightsSaveSvc", [
            "ENV",
            "$resource",
            OnesiteCloneRightsSaveSvc
        ]);
})(angular);