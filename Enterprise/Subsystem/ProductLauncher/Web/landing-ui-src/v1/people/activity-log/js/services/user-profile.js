//  Profile Data Service

(function (angular, undefined) {
    "use strict";

    function UserProfileDataSvc(ENV, $resource) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/profiles/:realPageId";

        defaults = {};

        actions = {
            save: {
                method: "PUT"
            }
        };

        resource = $resource(url, defaults, actions);

        svc.get = function (realPageId) {
            return $resource(url, {
                realPageId: realPageId
            }, actions).get.apply(resource, arguments).$promise;
        };
    }

    angular
        .module("settings")
        .service("userActivityLogProfileDataSvc", [
            "ENV",
            "$resource",
            UserProfileDataSvc
        ]);
})(angular);
