//  User Details Service

(function (angular) {
    "use strict";

    function userSvc($resource, ENV) {
        var svc = {};

        svc.url = {
            userDetails: ENV.landingAPI + "api/profiles/details",
            accountStatus: ENV.landingAPI + "api/userlogin/autologoutinterval",
            passwordStatus: ENV.landingAPI + "api/credential/checkpasswordexpiration"
        };

        svc.getUserProfile = function () {
            return $resource(svc.url.userDetails).get().$promise;
        };

        svc.getPasswordState = function () {
            return $resource(svc.url.passwordStatus).get().$promise;
        };

        svc.getAccountState = function (realPageId) {
            var params = {
                realPageId: realPageId
            };

            return $resource(svc.url.accountStatus, params).get().$promise;
        };

        return svc;
    }

    angular
        .module("settings")
        .factory("userDetailsSvc", ["$resource", "ENV", userSvc]);
})(angular);
