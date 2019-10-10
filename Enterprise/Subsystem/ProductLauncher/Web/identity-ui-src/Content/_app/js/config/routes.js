//  Configure Routes

(function (angular) {
    "use strict";

    function config(RoutesProvider) {
        var routes = {};

        routes["login"] = {
            url: "",
            controller: "LoginCtrl as page",
            templateUrl: "index.html",
            lazyLoad: [{
                rerun: true,

                serie: true,

                files: [
                    "identity.login.base",
                    "identity.user-lookup.base"
                ]
            }]
        };

        routes["user-lookup"] = {
            url: "",
            controller: "UserLookupCtrl as page",
            templateUrl: "index.html",
            lazyLoad: [{
                rerun: true,
                files: [
                    "identity.user-lookup.base"
                ]
            }]
        };

        routes["forgot-password"] = {
            url: "",
            controller: "ForgotPasswordCtrl as page",
            templateUrl: "index.html",
            lazyLoad: [{
                rerun: true,
                files: [
                    "identity.forgot-password.base"
                ]
            }]
        };

        routes["change-password"] = {
            url: "",
            controller: "ChangePwdController as page",
            templateUrl: "index.html",
            lazyLoad: [{
                rerun: true,
                files: [
                    "identity.change-password.base"
                ]
            }]
        };

        // RoutesProvider.setTemplateUrlPrefix("/Content/").setRoutes(routes).setDefault("/");
        RoutesProvider.setTemplateUrlPrefix("/login/content/").setRoutes(routes);
    }

    angular
        .module("identity")
        .config(["rpRoutesProvider", config]);
})(angular);
