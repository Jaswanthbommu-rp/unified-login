//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function UMAssignRolesToRightsSavesvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/products/unifiedlogin/right/roles";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("umAssignRolesToRightsSavesvc", [
            "ENV",
            "$resource",
            UMAssignRolesToRightsSavesvc
        ]);
})(angular);