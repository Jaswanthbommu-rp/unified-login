//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function OnesiteCloneRoleSaveSvc(ENV, $resource) {
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
        .service("onesiteCloneRoleSaveSvc", [
            "ENV",
            "$resource",
            OnesiteCloneRoleSaveSvc
        ]);
})(angular);