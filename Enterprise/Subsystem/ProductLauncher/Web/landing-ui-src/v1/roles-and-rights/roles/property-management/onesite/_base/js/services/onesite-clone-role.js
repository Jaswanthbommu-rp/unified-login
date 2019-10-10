//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function OnesiteCloneRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesite/role";

        actions = {
            save: {
                method: "POST"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("onesiteCloneRoleSvc", [
            "ENV",
            "$resource",
            OnesiteCloneRoleSvc
        ]);
})(angular);