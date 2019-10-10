(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("securityQuestions").translate(guid);
        };
    }

    angular
        .module("identity")
        .filter("securityQuestionsText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
 