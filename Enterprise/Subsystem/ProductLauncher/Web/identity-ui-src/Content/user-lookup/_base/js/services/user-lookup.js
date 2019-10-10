//  User Lookup Service

(function (angular) {
    "use strict";

    function userLookupSvc($resource, ENV) {
        var svc = {};        

        svc.lookupUser = function (username) {
            var request,
                actions;

            request = {
                url: ENV.landingAPI + "api/credential/GetSecurityQuestions"
            };

            actions = {
                lookupUser: {
                    method: "GET",
                    params: {
                        enterpriseUserName: username
                    }
                }
            };

            return $resource(request.url, {}, actions).lookupUser().$promise;
        };

        return svc;
    }

    angular
        .module("identity")
        .factory("userLookupSvc", [
            "$resource",
            "ENV",
            userLookupSvc
        ]);
})(angular);
