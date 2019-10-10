(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.manageUser.userDetails");

        bundle.set({

            user_detail_first_name: "First Name",
            user_detail_middle_init: "Middle Initial",
            user_detail_last_name: "Last Name",
            user_detail_username: "Username",
            user_detail_effective_date: "User Effective",
            user_detail_expiration_date: "User Expires",
            user_detail_password: "Password",
            user_detail_confirm_password: "Re-enter Password",

            add_persona: "Add Persona",
            enterprise_role: "Enterprise Role",
            notification_email: "Notification Email",
            username_email: "Username / Email",

            user_detail_optional: "optional",

            user_detail_enable_user: "Enable access for this user?",
            user_detail_user_type: "User Type",

            err_first_name_required: "First name is required",
            err_last_name_required: "Last name is required",
            err_username_required: "Username is required",
            err_user_type_required: "User type is required",
            err_notif_email_invalid: "Notification email should be a valid email address",
            err_pass_requirements: "Password should meet all requirements",
            err_pass_no_match: "Passwords do not match.",
            err_useremail_required: "Username/Email is required",
            err_password_required: "Password is required",
            err_password_sameusername: "Password cannot be the same as username",
            
            change_pwd_char_range: "{{min}} - {{max}} characters",
            change_pwd_upper_case: "1 upper-case letter",
            change_pwd_lower_case: "1 lower-case letter",
            change_pwd_numerical: "1 number",
            change_pwd_special_char: "1 special character (such as !@#$%)",
            change_pwd_instructions: "Password must have at least:"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();