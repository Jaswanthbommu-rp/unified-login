(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "product_access_details",
            "product_access_properties",
            "product_access_roles",

            "product_access_no_products",
            "product_access_select_one"
        ];

        appLangKeys.app("people.manageUser.productAccess").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
