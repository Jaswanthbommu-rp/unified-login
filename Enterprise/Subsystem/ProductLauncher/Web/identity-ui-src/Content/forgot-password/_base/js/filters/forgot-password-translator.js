(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("forgotPassword").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("forgotPasswordText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 