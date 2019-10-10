(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

        	"forgot_instruction",
        	
        	"forgot_continue",
        	"forgot_back_to_login",

        	"forgot_system_err_contact_admin"

        ];

        appLangKeys.app("forgotPassword").set(keys);
    }

    angular
        .module("identity")
        .config(["appLangKeysProvider", config]);
})();
