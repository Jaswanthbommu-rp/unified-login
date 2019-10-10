// Validate Service

(function (angular) {
    "use strict";

    function validateUserSvc($resource, ENV) {
        var svc = {};        

        svc.validateUser = function (params) {
            var request,
                actions;

            request = {
                url: ENV.landingAPI + "api/user/validate"
            };

            actions = {
                validateUser: {
                    method: "GET",
                    params: {
                        enterpriseUserName: params.enterpriseUserName,
                        newUserRegistrationToken: params.newUserRegistrationToken
                    }
                }
            };

            return $resource(request.url, {}, actions).validateUser().$promise;
        };

        return svc;
    }

    angular
        .module("new-user")
        .factory("validateUserSvc", [
            "$resource",
            "ENV",
            validateUserSvc
        ]);
})(angular);
