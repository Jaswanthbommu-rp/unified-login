//  Configure Routes

(function (angular) {
    "use strict";

    function config(RoutesProvider) {
        var routes = {};

        routes["validate"] = {
            url: "/validate/:newUserRegistrationToken/:enterpriseUserName",
            controller: "ValidateCtrl as page",
            lazyLoad: [{
                files: [
                    "new-user.validate"
                ]
            }]
        };

        routes["validate-token"] = {
            url: "/validate-token/:validateUserToken/:enterpriseUserName",
            controller: "ValidateTokenCtrl as page",
            lazyLoad: [{
                files: [
                    "new-user.validate-token"
                ]
            }]
        };

        routes["set-password"] = {
            url: "/set-password",
            controller: "ChangePwdController as page",
            resolve: ["user"],
            lazyLoad: [{
                files: [
                    "new-user.set-password.base"
                ]
            }]
        };

        routes["security-questions"] = {
            url: "/security-questions",
            controller: "SecurityQuestionsCtrl as page",
            resolve: ["user"],
            lazyLoad: [{
                files: [
                    "new-user.security-questions"
                ]
            }]
        };

        routes["start-profile"] = {
            url: "/start-profile",
            controller: "StartProfileCtrl as page",
            resolve: ["user"],
            lazyLoad: [{
                files: [
                    "new-user.start-profile"
                ]
            }]
        };

        routes["error"] = {
            url: "/error",
            controller: "ErrorCtrl as page",
            lazyLoad: [{
                files: [
                    "new-user.error"
                ]
            }]
        };

        RoutesProvider.setRoutes(routes).setDefault("/error");
    }

    angular
        .module("new-user")
        .config(["rpRoutesProvider", config]);
})(angular);
