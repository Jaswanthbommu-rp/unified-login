(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("profileSetup").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("startProfileText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 