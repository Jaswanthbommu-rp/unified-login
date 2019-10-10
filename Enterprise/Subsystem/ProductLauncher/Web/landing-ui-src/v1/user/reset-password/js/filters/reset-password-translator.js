(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        var translator = appLangTranslate("user.resetPassword");

        return function () {
            return translator.translate.apply(translator.translate, arguments);
        };
    }

    angular
        .module("settings")
        .filter("resetPasswordText", [
            "appLangTranslate",
            filter
        ]);
})(angular);

