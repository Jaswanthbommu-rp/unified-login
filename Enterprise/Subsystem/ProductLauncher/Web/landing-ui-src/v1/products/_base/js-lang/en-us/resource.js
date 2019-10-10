(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("products");

        bundle.set({
            "products.learnMore": "Learn More",
            "products.filters.familyDefault": "All Product Families",
            "products.filters.solutionDefault": "All Product Solutions",
            "products.filters.searchTextPlaceholder": "Search by product name"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
