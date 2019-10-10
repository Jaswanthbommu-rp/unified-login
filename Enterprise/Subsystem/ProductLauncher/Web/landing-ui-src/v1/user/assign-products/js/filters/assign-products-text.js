// Assign Products

(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        var translator = appLangTranslate("assignProducts");

        return function (id) {
            return translator.translate(id);
        };
    }

    angular
        .module("settings")
        .filter("assignProductsText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
