(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("people.user-list").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("userListText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 