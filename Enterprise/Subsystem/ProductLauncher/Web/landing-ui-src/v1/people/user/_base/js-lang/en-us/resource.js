(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.manageUser");

        bundle.set({

            //TODO temporary: workaround while labels in the data are still different
            user_type_regular_user: "Regular User",
            user_type_regular_no_email_user: "Regular User (no email)",
            user_type_super_user: "RealPage System Administrator",
            user_type_employee : "RealPage Employee",
            user_type_sde: "SDE",
            user_type_external_user: "External User",
            user_type_rp_employee: "RealPage Employee",

        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();