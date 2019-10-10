//  New Role Rights Save Service

(function(angular, undefined) {
    "use strict";

    function CompAdminRefreshMsgSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/user/assignproductstoadministrators";

        actions = {
            save: {
                method: "POST"
            }
        };

        return $resource(url, {}, actions);        
    }

    angular
        .module("settings")
        .service("compAdminRefreshMsgSvc", [
            "ENV",
            "$resource",            
            CompAdminRefreshMsgSvc
        ]);
})(angular);