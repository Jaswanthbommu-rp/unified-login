(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "search_by_name",
            "select_all",
            "no_products",
            "loading_products"

        ];

        appLangKeys.app("people.manageUser.addProducts").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
