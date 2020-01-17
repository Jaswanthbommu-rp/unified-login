//  Header Username

(function (angular, undefined) {
    "use strict";

    function GlobalHeaderUsername(userModel, persona, headerModel) {
        var svc = this;

        svc.nameWatch = angular.noop;
        svc.personaWatch = angular.noop;

        svc.bind = function () {
            svc.nameWatch();
            svc.personaWatch();
            svc.nameWatch = userModel.subscribe(svc.setData);
            svc.personaWatch = persona.subscribe(svc.setCompanyName);
        };

        svc.setData = function () {
            headerModel.extendData({
                username: userModel.getFullName(),
                userInitials: userModel.getInitials()
            });
        };

        svc.setCompanyName = function (data) {
            headerModel.extendData({
                "prodName": "RealPage Unified Platform",
                "companyName": persona.getCompanyName(),
                "propertyName": "",
                "userRole": persona.getUserRole()
            });
            // var omnibar = document.querySelector('raul-scope');
            // omnibar.scopeProduct="RealPage Unified Platform";
            // omnibar.scopeBase = persona.getCompanyName();
            // omnibar.scopeSelected = "";
        };
    }

    angular
        .module("settings")
        .service("globalHeaderUsername", [
            "userSessionModel",
            "personaDetails",
            "rpGlobalHeaderModel",
            GlobalHeaderUsername
        ]);
})(angular);
