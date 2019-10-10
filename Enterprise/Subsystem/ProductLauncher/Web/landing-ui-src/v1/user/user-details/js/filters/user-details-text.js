// User Details Text

(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        var translator = appLangTranslate("user.userDetails");

        return function () {
            return translator.translate.apply(translator.translate, arguments);
        };
    }

    angular
        .module("settings")
        .filter("userDetailsText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
