(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("setPassword").translate(guid);
        };
    }

    angular
        .module("new-user")
        .filter("setPasswordText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 