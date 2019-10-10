(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("manageProfile");

        bundle.set({
            cancelBtn: "Cancel",
            updateBtn: "Update",
            profileTab: "Profile",
            modalTitle: "Update User Profile",
            resetPasswordTab: "Reset Password",
            securityQuestionsTab: "Security Questions"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();