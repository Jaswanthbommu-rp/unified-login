//  New Role Save Service

(function(angular, undefined) {
    "use strict";

    function SpndMgmtNewRoleSaveSvc(ENV, $resource, user) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/ops/role/";

        defaults = {};

        actions = {
            save: {
                method: "POST"
            }
        };

        resource = $resource(url, defaults, actions);

        svc.save = function(data) {
            return $resource(url, angular.extend(defaults, data), actions).save.apply(resource, arguments).$promise;
        };
    }

    angular
        .module("settings")
        .service("spndMgmtNewRoleSaveSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            SpndMgmtNewRoleSaveSvc
        ]);
})(angular);