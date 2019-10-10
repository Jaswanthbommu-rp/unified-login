//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function AcctCloneRoleSaveSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesiteaccounting/role";

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("acctCloneRoleSaveSvc", [
            "ENV",
            "$resource",
            AcctCloneRoleSaveSvc
        ]);
})(angular);