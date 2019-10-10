(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("people.changePassword").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("changePasswordText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 