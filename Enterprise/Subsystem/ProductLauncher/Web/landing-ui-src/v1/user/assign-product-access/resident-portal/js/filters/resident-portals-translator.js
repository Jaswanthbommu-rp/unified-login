(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("residentPortals").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("residentPortalsText", [
            "appLangTranslate",
            filter
        ]);
})(angular);

