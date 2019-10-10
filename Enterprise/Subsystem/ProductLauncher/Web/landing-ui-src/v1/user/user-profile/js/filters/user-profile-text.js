// User Details Text

(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        var translator = appLangTranslate("userEditProfile");

        return function () {
            return translator.translate.apply(translator.translate, arguments);
        };
    }

    angular
        .module("settings")
        .filter("userEditProfileText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
