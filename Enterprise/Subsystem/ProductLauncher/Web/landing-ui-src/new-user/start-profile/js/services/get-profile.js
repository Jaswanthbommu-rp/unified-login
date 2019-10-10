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
            if (realPageId !== null) {
                return $resource(url, {
                    realPageId: realPageId
                }, actions).get.apply(resource, arguments).$promise;
            }
            else {
                return resource.get.apply(resource, arguments).$promise;
            }
        };

        svc.save = function (realPageId, data) {
            if (realPageId !== null) {
                return $resource(url, {
                    realPageId: realPageId
                }, actions).save.apply(resource, arguments).$promise;
            }
            else {
                return resource.save.apply(resource, arguments).$promise;
            }
        };
    }

    angular
        .module("new-user")
        .service("userProfileDataSvc", [
            "ENV",
            "$resource",           
            UserProfileDataSvc
        ]);
})(angular);
