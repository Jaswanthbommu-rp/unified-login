(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "products.learnMore",
            "products.filters.familyDefault",
            "products.filters.solutionDefault",
            "products.filters.searchTextPlaceholder"
        ];

        appLangKeys.app("products").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
