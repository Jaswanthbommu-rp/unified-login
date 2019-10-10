(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("people.user.edit").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("editUserText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 