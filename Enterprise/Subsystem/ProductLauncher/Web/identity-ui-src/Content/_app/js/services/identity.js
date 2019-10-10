//  Identity Service

(function (angular) {
    "use strict";

    function IdentitySvc(identityModel) {
        var svc = this;

        svc.init = function () {
            svc.data = angular.copy(identityModel);
        };
    }

    angular
        .module("identity")
        .service("identitySvc", [
            "Model",
            IdentitySvc
        ]);
})(angular);
