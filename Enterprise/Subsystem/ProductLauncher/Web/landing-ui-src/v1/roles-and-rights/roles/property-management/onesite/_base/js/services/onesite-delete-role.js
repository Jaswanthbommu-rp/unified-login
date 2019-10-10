//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function OnesiteDeleteRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesite/role";

        actions = {
            save: {
                method: "DELETE"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("onesiteDeleteRoleSvc", [
            "ENV",
            "$resource",
            OnesiteDeleteRoleSvc
        ]);
})(angular);