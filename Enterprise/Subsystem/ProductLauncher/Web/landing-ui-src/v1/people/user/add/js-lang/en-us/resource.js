(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.user.add");

        bundle.set({

            cancel: "Cancel",
            create_user: "Save & Assign Products",
            user_details: "Add New User"
                        
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();