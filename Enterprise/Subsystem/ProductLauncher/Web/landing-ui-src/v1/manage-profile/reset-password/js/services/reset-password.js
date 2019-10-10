//  Reset Password Service

(function (angular, undefined) {
    "use strict";

    function ResetPasswordSvc(ENV, $resource, user, userProfileModel) {
        var svc = this,
            res = $resource(ENV.landingAPI + "api/credential/resetpassword");

        svc.reset = function (realpageId) {
            if(realpageId !== null && userProfileModel.isProfileCardCaller()){
                return $resource(ENV.landingAPI + "api/credential/resetpassword?realPageId=" + realpageId).save.apply(res.save, arguments).$promise;
            }else{
                return res.save.apply(res.save, arguments).$promise;
            }
        };
    }

    angular
        .module("settings")
        .service("mpResetPasswordSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            "userProfileModel",
            ResetPasswordSvc
        ]);
})(angular);
