(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("people.manageUser.productAccess").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("manageUserProductAccessText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 