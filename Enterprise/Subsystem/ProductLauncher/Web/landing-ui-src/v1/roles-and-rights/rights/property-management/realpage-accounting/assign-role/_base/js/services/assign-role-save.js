//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function AcctAssignRolesToRightsSavesvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/products/onesiteaccounting/right/roles";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("acctAssignRolesToRightsSavesvc", [
            "ENV",
            "$resource",
            AcctAssignRolesToRightsSavesvc
        ]);
})(angular);