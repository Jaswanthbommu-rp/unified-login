(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("userLookup").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("userLookupText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 