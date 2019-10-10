//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function AcctNewSaveRightsSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesiteaccounting/role/rights";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);

    }

    angular
        .module("settings")
        .service("acctNewSaveRightsSvc", [
            "ENV",
            "$resource",
            AcctNewSaveRightsSvc
        ]);
})(angular);