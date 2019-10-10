(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("resetPassword").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("resetPasswordText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 