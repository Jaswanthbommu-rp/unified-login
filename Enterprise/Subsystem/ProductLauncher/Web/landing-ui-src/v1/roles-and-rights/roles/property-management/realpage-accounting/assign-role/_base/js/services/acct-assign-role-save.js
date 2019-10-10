//  New Role Save Service

(function(angular, undefined) {
    "use strict";

    function AcctAssignRoleSaveSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/products/onesiteaccounting/role";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }
    angular
        .module("settings")
        .service("acctAssignRoleSaveSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            AcctAssignRoleSaveSvc
        ]);
})(angular);