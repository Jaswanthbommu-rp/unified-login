//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function AcctCloneRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesiteaccounting/role";

        actions = {
            save: {
                method: "POST"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("acctCloneRoleSvc", [
            "ENV",
            "$resource",
            AcctCloneRoleSvc
        ]);
})(angular);