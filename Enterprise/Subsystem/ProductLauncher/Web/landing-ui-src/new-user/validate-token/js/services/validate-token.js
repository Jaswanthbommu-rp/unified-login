// Validate Service

(function (angular) {
    "use strict";

    function validateTokenSvc($resource, ENV) {
        var svc = {};        

        svc.validate = function (params) {
            var request,
                actions;

            request = {
                url: ENV.landingAPI + "api/user/validate-token"
            };

            actions = {
                validateToken: {
                    method: "GET",
                    params: {
                        enterpriseUserName: params.enterpriseUserName,
                        verificationToken: params.validateUserToken
                    }
                }
            };

            return $resource(request.url, {}, actions).validateToken().$promise;
        };

        return svc;
    }

    angular
        .module("new-user")
        .factory("validateTokenSvc", [
            "$resource",
            "ENV",
            validateTokenSvc
        ]);
})(angular);
