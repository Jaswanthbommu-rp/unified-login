(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("newUserValidationToken").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("validationTokenText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 