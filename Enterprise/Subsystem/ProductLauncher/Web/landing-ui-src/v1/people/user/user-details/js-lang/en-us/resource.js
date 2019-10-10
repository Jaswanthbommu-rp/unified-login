(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.manageUser.userDetails");

        bundle.set({

            user_detail_first_name: "First",
            user_detail_middle_init: "Middle Initial",
            user_detail_last_name: "Last",
            user_detail_username: "Username",
            user_detail_effective_date: "Effective",
            user_detail_expiration_date: "Expires",
            user_detail_password: "Password",

            add_persona: "Add Persona",
            enterprise_role: "Enterprise Role",
            notification_email: "Notification Email",
            username_email: "Username / Email",

            user_detail_optional: "Optional",

            user_detail_enable_user: "Enabled",
            user_detail_user_type: "User Type",

            user_detail_regular_user: "Regular",
            user_detail_regular_no_email_user: "Regular User (no email)",
            user_detail_super_user: "RealPage System Administrator",
            user_detail_employee: "RealPage Employee",
            user_detail_sde: "SDE",
            user_detail_external_user: "External User",
            user_detail_rp_employee: "RealPage Employee",

            err_first_name_required: "First name is required",
            err_last_name_required: "Last name is required",
            err_username_required: "Username is required",
            err_user_type_required: "User type is required"
            
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();