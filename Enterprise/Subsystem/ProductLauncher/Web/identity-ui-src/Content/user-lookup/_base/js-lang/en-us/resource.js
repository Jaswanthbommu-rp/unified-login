(function() {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("userLookup");

        bundle.set({

            lookup_username_info: "Your Username is normally your company email address or an alternative specified by your system administrator.",
            lookup_enter_username: "Enter your username",

            lookup_required: "This field is required",
            lookup_continue: "Continue",
            lookup_back_to_login: "Return to login screen",
            lookup_system_err_contact_admin: "A system error has occurred. Please contact your system administrator."
        });

        bundle.test();
    }

    angular
        .module("identity")
        .config(["appLangBundleProvider", config]);
})();
