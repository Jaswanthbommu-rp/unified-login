//  Rights Data Service

(function(angular, undefined) {
    "use strict";

    function AssignRolesToRightsSvc(ENV, $resource, user) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesite/right/roles";

        defaults = {
            realPageId: user.getRealPageId()
        };

        actions = {
            save: {
                method: "GET"
            }
        };


        resource = $resource(url, defaults, actions);

        svc.getData = function(realPageId) {
            return resource.get.apply(resource, arguments).$promise;
        };

        svc.save = function(realPageId, data) {
            return resource.save.apply(resource, arguments).$promise;

        };
    }

    angular
        .module("settings")
        .service("assignRolesToRightsSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            AssignRolesToRightsSvc
        ]);
})(angular);