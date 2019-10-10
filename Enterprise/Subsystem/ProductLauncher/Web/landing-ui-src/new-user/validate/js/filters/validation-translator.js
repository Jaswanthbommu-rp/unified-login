(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("newUserValidation").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("validationText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 