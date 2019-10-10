//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function AssignRolesToRightsSavesvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/products/onesite/right/roles";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("assignRolesToRightsSavesvc", [
            "ENV",
            "$resource",
            AssignRolesToRightsSavesvc
        ]);
})(angular);