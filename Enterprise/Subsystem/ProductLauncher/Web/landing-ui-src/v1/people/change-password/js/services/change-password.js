(function (angular) {
    "use strict";

    var changePasswordSvc = function ($q, $resource, $window, userModel, ENV) {
        var svc = {};

        svc.url = {
            saveNewPassword: ENV.landingAPI + "api/credential/resetpassword",
            checkNewPassword: ENV.landingAPI + "api/credential/validatepassword"
        };

        svc.savePassword = function (passwordData) {
            var actions = {
                    update: {
                        method: "POST"
                    }
                },
                paramData = {
                    newPassword: passwordData.createPassword,
                    oldPassword: userModel.getRealPageId()
                };

            return $resource(svc.url.saveNewPassword, {}, actions).update(paramData).$promise;
        };

        svc.isUnique = function (newPassword) {
            var actions = {
                    post: {
                        method: "POST"
                    }
                },
                paramData = {
                    enterpriseUserName: userModel.getUsername(),
                    passwordToValidate: newPassword
                };

            angular.forEach(paramData, function (val, key) {
                paramData[key] = $window.encodeURIComponent(val);
            });

            return $resource(svc.url.checkNewPassword, paramData, actions).post().$promise;
        };
        return svc;
    };

    angular
        .module("settings")
        .factory("changePasswordSvc", [
            "$q",
            "$resource",
            "$window",
            "userSessionModel",
            "ENV",
            changePasswordSvc
        ]);
})(angular);
