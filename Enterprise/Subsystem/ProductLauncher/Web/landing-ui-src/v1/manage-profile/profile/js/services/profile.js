//  Profile Data Service

(function (angular, undefined) {
    "use strict";

    function MpProfileTabDataSvc(ENV, $resource, user, userProfileModel) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "api/profiles/:realPageId";

        defaults = {
            realPageId: user.getRealPageId()
        };

        actions = {
            save: {
                method: "PUT"
            }
        };

        resource = $resource(url, defaults, actions);

        svc.get = function (realPageId) {
            if(realPageId !== null && userProfileModel.isProfileCardCaller()){ //edit user manage profile
                return $resource(url, {realPageId:realPageId}, actions).get.apply(resource, arguments).$promise;
            }else{
                return resource.get.apply(resource, arguments).$promise;
            }
        };

        svc.save = function (realPageId, data) {
            if(realPageId !== null && userProfileModel.isProfileCardCaller()){ //edit user manage profile
                return $resource(url, {realPageId:realPageId}, actions).save.apply(resource, arguments).$promise;
            }else{
                return resource.save.apply(resource, arguments).$promise;
            }
        };
    }

    angular
        .module("settings")
        .service("mpProfileTabDataSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            "userProfileModel",
            MpProfileTabDataSvc
        ]);
})(angular);
