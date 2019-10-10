(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.user.edit");

        bundle.set({

            cancel: "Cancel",
            clone_user: "Clone User",
            update_user: "Update User",
            
            err_update_user: "Unable update user."
            
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();