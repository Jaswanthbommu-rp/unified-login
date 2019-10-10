(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("common.notification-lang").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("notificationText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 