//  New Role Save Service

(function (angular, undefined) {
    "use strict";

    function UserMgmtNewRoleSaveSvc(ENV, $resource, user) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/unifiedlogin/role/";

        defaults = {};

        actions = {
            save: {
                method: "POST"
            }
        };

        resource = $resource(url, defaults, actions);
        
        svc.save = function (data) {                
                return $resource(url, angular.extend(defaults,data), actions).save.apply(resource, arguments).$promise;            
        };
    }

    angular
        .module("settings")
        .service("userMgmtNewRoleSaveSvc", [
            "ENV",
            "$resource",
            "userSessionModel",            
            UserMgmtNewRoleSaveSvc
        ]);
})(angular);
