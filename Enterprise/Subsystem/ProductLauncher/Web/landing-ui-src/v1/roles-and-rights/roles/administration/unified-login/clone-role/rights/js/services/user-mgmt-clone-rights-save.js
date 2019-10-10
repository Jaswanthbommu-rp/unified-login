//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function UserMgmtCloneRightsSaveSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            // url = ENV.landingAPI + "/api/products/unifiedlogin/role/rights";
            url = ENV.landingAPI + "/api/products/unifiedlogin/clone/rights";            

        actions = {
            save: {
                method: "PUT"
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .service("userMgmtCloneRightsSaveSvc", [
            "ENV",
            "$resource",            
            UserMgmtCloneRightsSaveSvc
        ]);
})(angular);