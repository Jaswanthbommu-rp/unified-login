(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "assign_products",
            "product_access",

            "persona_primary",
            "persona_name",
            "persona_type",
            "persona_effective_date",
            "persona_expiry_date",
            "persona_optional",

            "err_req_persona_type"

        ];

        appLangKeys.app("people.manageUser.persona").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
