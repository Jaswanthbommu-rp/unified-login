//  New Role Save Service

(function(angular, undefined) {
    "use strict";

    function OnesiteAssignRoleSaveSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/products/onesite/role";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }
    angular
        .module("settings")
        .service("onesiteAssignRoleSaveSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            OnesiteAssignRoleSaveSvc
        ]);
})(angular);