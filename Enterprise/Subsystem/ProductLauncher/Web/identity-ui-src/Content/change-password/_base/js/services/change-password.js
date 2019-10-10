(function (angular) {
    "use strict";

    var changePasswordSvc = function($resource, userModel, ENV) {
        var svc = {};

        svc.url = {
            saveNewPassword: ENV.landingAPI + "api/credential/forgotpassword",
            checkNewPassword: ENV.landingAPI + "api/credential/validatepassword"
        };

        svc.savePassword = function(changePasswordModel) {
            var actions = {
                    post: { method: "POST" }
                }, 
                paramData = {
                    enterpriseUserName: userModel.getEnterpriseLoginName(),
                    activityToken: userModel.getActivityToken(),
                    newPassword: changePasswordModel.createPassword,
                    correctAnswerToken: userModel.getCorrectAnswerToken()
                };
            return $resource(svc.url.saveNewPassword, {}, actions).post(paramData).$promise;
        };

        svc.isUnique = function (newPassword) {
            var actions = {
                post: { method: "POST" }
            },
                paramData = {
                    enterpriseUserName: userModel.getEnterpriseLoginName(),
                    passwordToValidate: newPassword
                };
            return $resource(svc.url.checkNewPassword, paramData, actions).post().$promise;
        };

        return svc;
    };

    angular
        .module("identity")
        .factory("changePasswordSvc", [
            "$resource",
            "userModel",
            "ENV",
            changePasswordSvc
        ]);
})(angular);