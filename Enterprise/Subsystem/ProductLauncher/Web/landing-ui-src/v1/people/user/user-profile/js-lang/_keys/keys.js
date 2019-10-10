(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "user_detail_first_name",
            "user_detail_middle_init",
            "user_detail_last_name",
            "user_detail_username",
            "user_detail_effective_date",
            "user_detail_expiration_date",
            "user_detail_password",
            "user_detail_confirm_password",

            "add_persona",
            "enterprise_role",
            "notification_email",
            "username_email",

            "user_detail_optional",

            "user_detail_enable_user",
            "user_detail_user_type",

            "err_first_name_required",
            "err_last_name_required",
            "err_username_required",
            "err_user_type_required",
            "err_notif_email_invalid",
            "err_pass_requirements",
            "err_pass_no_match",
            "err_useremail_required",
            "err_password_required",
            "err_password_sameusername",

            "change_pwd_char_range",
            "change_pwd_upper_case",
            "change_pwd_lower_case",
            "change_pwd_numerical",
            "change_pwd_special_char",
            "change_pwd_instructions"
        ];

        appLangKeys.app("people.manageUser.userDetails").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
