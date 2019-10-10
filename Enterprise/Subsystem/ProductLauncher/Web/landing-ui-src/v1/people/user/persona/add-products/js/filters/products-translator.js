(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("people.manageUser.addProducts").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("manageUserProductsText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 