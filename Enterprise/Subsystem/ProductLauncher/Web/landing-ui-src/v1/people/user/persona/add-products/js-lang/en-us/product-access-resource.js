(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.manageUser.productAccess");

        bundle.set({

            product_access_details: "Details",
            product_access_properties: "Properties",
            product_access_roles: "Roles",

            product_access_no_products: "No products available.",
            product_access_select_one: "Please select a product first."

        });

        bundle.test();
        
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();