//  New Role Save Service

(function(angular, undefined) {
    "use strict";

    function AcctNewRoleSaveSvc(ENV, $resource, user) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesiteaccounting/role/";

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
        .service("acctNewRoleSaveSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            AcctNewRoleSaveSvc
        ]);
})(angular);