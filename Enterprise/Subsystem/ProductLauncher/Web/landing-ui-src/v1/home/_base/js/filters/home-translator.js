(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("dashboard").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("dashboardText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
