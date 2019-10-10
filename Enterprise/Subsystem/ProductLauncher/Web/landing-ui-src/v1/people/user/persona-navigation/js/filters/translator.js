(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("people.manageUser.personaNavigation").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("personaNavigationText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 