(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("changePassword").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("changePasswordText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 