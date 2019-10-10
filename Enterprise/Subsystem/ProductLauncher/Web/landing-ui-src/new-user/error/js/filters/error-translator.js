(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("newUserError").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("errorText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 