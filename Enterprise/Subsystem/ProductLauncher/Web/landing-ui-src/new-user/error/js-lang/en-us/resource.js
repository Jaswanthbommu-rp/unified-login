(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("newUserError");

        bundle.set({
            system_err_contact_admin: "A system error has occurred. Please contact your system administrator."
        });

        bundle.test();
    }

    angular
        .module("new-user")
        .config(["appLangBundleProvider", config]);
})();