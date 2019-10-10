//  Layout Model

(function (angular) {
    "use strict";

    function factory($state, cdnVer) {
        var model = {};

        // model.states = {
        //     loginForm: {
        //         templatePath: "/Content/login/index.html"
        //     },
        //     userLookupForm: {
        //         templatePath: "/Content/user-lookup/index.html"
        //     },
        //     forgotPasswordForm: {
        //         templatePath: "/Content/forgot-password/index.html"
        //     },
        //     changePasswordForm: {
        //         templatePath: "/Content/change-password/index.html"
        //     }
        // };

        model.init = function () {
            // model.currentState = model.states.loginForm;

            model.setActiveState("login");
            model.cdnVer = cdnVer;
            model.svgPath = "/login/" + model.cdnVer + "/lib/realpage/svg-icons/images";

            return model;
        };

        model.setActiveState = function (stateName) {
            $state.go(stateName);

            return model;
        };

        model.reset = function () {
            return model;
        };

        return model.init();
    }

    angular
        .module("identity")
        .factory("layoutModel", [
            "$state",
            "cdnVer",
            factory
        ]);
})(angular);
