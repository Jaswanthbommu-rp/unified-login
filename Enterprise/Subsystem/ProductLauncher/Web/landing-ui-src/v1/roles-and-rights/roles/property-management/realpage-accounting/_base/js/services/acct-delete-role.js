//  Clone Role Save Service

(function(angular, undefined) {
    "use strict";

    function AcctDeleteRoleSvc(ENV, $resource) {
        var actions,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesiteaccounting/role";

        actions = {
            save: {
                method: "DELETE"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("acctDeleteRoleSvc", [
            "ENV",
            "$resource",
            AcctDeleteRoleSvc
        ]);
})(angular);