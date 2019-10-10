(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.manageUser.addProducts");

        bundle.set({

            search_by_name: "Search by Name",
            select_all: "Select All",
            no_products: "No products available.",
            loading_products: "Loading products."

        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();