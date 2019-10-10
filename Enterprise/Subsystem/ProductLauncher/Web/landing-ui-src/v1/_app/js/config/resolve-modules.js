//  Config Resolve Module

(function (angular) {
    "use strict";

    function config(resolveModule) {
        var resolve = {};

        resolve["appLayout"] = ["appLayoutSvc", function (layout) {
            return layout.init();
        }];

        resolve["checkUserStatus"] = ["userStatusSvc", function (userStatusSvc) {
            return userStatusSvc.checkPasswordStatus();
        }];

        resolve["security"] = ["routeSecurity", function (routeSecurity) {
            return routeSecurity.load();
        }];

        resolve["loadUserForPassword"] = ["passwordDetails", function (passwordDetails) {
            return passwordDetails.load();
        }];

        resolveModule.setResolve(resolve);
    }

    angular
        .module("settings")
        .config(["rpResolveModuleProvider", config]);
})(angular);
