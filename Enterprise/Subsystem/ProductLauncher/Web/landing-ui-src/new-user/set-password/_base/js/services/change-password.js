(function (angular) {
    "use strict";

    var changePasswordSvc = function ($resource, $window, userModel, ENV) {
        var svc = {};

        svc.url = {
            saveNewPassword: ENV.landingAPI + "api/credential/setpassword",
            checkNewPassword: ENV.landingAPI + "api/credential/validatepassword"
        };

        svc.save = function (changePasswordModel) {
            var actions = {
                    post: {
                        method: "POST"
                    }
                },
                paramData = {
                    enterpriseUserName: userModel.getEnterpriseUserName(),
                    activityToken: userModel.getActivityToken(),
                    newPassword: changePasswordModel.createPassword
                };
            return $resource(svc.url.saveNewPassword, {}, actions).post(paramData).$promise;
        };

        svc.validatePassword = function (newPassword, callback) {
            var res,

                actions = {
                    post: {
                        method: "POST",
                        cancellable: true
                    }
                },

                paramData = {
                    passwordToValidate: newPassword,
                    enterpriseUserName: userModel.getEnterpriseUserName()
                };

            angular.forEach(paramData, function (val, key) {
                paramData[key] = $window.encodeURIComponent(val);
            });

            res = $resource(svc.url.checkNewPassword, paramData, actions);

            return res.post(callback);
        };

        return svc;
    };

    angular
        .module("new-user")
        .service("changePasswordSvc", [
            "$resource",
            "$window",
            "userModel",
            "ENV",
            changePasswordSvc
        ]);
})(angular);
