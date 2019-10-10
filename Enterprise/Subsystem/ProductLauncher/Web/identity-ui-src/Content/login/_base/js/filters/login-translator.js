(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("login").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("loginText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 