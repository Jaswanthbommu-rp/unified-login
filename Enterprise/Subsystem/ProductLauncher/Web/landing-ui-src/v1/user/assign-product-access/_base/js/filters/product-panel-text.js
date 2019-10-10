// Product Panel Text

(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        var translator = appLangTranslate("assignProductAccess");

        return function (id) {
            return translator.translate(id);
        };
    }

    angular
        .module("settings")
        .filter("productPanelText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
