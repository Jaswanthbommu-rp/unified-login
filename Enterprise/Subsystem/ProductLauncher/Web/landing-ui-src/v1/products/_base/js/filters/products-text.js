// Language Translation Filter

(function (angular) {
    "use strict";

    function filter(appLangTranslate) {
        return function (id) {
            return appLangTranslate("products").translate(id);
        };
    }

    angular
        .module("settings")
        .filter("productsText", ["appLangTranslate", filter]);
})(angular);
