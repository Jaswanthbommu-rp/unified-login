// Manage Profile Translator

(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (key) {
            return appLangTranslate("manageProfile").translate(key);
        };
    }

    angular
        .module("settings")
        .filter("manageProfileText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
