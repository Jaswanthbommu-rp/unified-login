(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("forgotPassword");

        bundle.set({

        	forgot_instruction: "Answer the 2 random questions below to verify the account is yours and reset your password.",
        	
        	forgot_continue: "Continue",
        	forgot_back_to_login: "Return to login screen",

        	forgot_system_err_contact_admin: "A system error has occurred. Please contact your system administrator."

        });

        bundle.test();
    }

    angular
        .module("identity")
        .config(["appLangBundleProvider", config]);
})();