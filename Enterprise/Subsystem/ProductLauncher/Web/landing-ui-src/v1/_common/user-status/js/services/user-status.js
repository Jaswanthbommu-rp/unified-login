//  User Status Service

(function (angular, undefined) {
    "use strict";

    function UserStatusSvc($q, $window, $location, $state, timeout, session) {
        var svc = this;

        svc.sessionWatch = angular.noop;

        svc.checkPasswordStatus = function () {
            svc.resolveReq = $q.defer();

            if (session.isReady()) {
                svc.onSessionReady();
            }
            else {
                svc.sessionWatch = session.subscribe(svc.onSessionReady);
            }

            return svc.resolveReq.promise;
        };

        svc.onSessionReady = function () {
            if (svc.sessionWatch) {
                svc.sessionWatch();
                svc.sessionWatch = undefined;
            }

            timeout(function () {
                svc.redirectUser();
            });
        };

        svc.redirectUser = function () {
            var redirect = svc.getRedirectStatus();

            if (redirect.required) {
                $state.go("people.change-password", {
                    userId: session.getRealPageId()
                });
                svc.resolveReq.reject();
            }
            else {
                svc.resolveReq.resolve();
            }
        };

        svc.getRedirectStatus = function () {
            var redirectReqd = false,
                data = session.getData(),
                loginExpired = data.userLogin.isExpired,
                forceReset = data.userLogin.isForceReSetPassword,
                passwordExpired = data.passwordExpirationDetail.isPasswordExpired,
                url = "/people/users/:userId/change-password";

            redirectReqd = loginExpired || forceReset || passwordExpired;

            return {
                required: redirectReqd,
                url: url.replace(":userId", session.getRealPageId())
            };
        };
    }

    angular
        .module("settings")
        .service("userStatusSvc", [
            "$q",
            "$window",
            "$location",
            "$state",
            "timeout",
            "userSessionModel",
            UserStatusSvc
        ]);
})(angular);
