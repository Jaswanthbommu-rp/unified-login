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

            "add_persona",
            "enterprise_role",
            "notification_email",
            "username_email",

            "user_detail_optional",

            "user_detail_enable_user",
            "user_detail_user_type",

            "user_detail_regular_user",
            "user_detail_regular_no_email_user",
            "user_detail_super_user",
            "user_detail_employee",
            "user_detail_sde",
            "user_detail_external_user",
            "user_detail_rp_employee",

            "err_first_name_required",
            "err_last_name_required",
            "err_username_required",
            "err_user_type_required",
        ];

        appLangKeys.app("people.manageUser.userDetails").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
