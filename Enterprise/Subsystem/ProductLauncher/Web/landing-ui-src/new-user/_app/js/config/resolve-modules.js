//  Config Resolve Module

(function (angular) {
    "use strict";

    function config(resolveModule) {
        var resolve = {};

        resolve["user"] = ["userModel", function (user) {
            return user.checkUserValidated();
        }];

        resolveModule.setResolve(resolve);
    }

    angular
        .module("new-user")
        .config(["rpResolveModuleProvider", config]);
})(angular);
