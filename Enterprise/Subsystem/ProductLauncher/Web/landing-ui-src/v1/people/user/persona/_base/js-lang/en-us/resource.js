(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.manageUser.persona");

        bundle.set({

            "assign_products": "Assign Products",
            "product_access": "Product Access",

            "persona_primary": "Primary",
            "persona_name": "Persona Name",
            "persona_type": "Type",
            "persona_effective_date": "Persona Effective",
            "persona_expiry_date": "Persona Expires",
            "persona_optional": "optional",

            "err_req_persona_type": "Persona Type is required."

        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();