(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("people.manageUser.userDetails").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("userDetailsText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 