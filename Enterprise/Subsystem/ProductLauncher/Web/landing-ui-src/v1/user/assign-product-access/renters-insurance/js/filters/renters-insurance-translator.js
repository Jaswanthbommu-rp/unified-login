(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (guid) {
            return appLangTranslate("rentersInsurance").translate(guid);
        };
    }

    angular
        .module("settings")
        .filter("rentersInsuranceText", [
            "appLangTranslate",
            filter
        ]);
})(angular);
