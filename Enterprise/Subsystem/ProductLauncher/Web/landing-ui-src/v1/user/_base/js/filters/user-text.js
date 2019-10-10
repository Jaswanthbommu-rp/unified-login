// User Text

(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        var translator = appLangTranslate("user.base");

        return function (id) {
            return translator.translate(id);
        };
    }

    angular
        .module("settings")
        .filter("userBaseText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
