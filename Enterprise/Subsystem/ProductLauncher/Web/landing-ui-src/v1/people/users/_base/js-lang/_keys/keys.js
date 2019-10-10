(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            //select all dropdown
            "users_select_visible",
            "users_select_all",

            //grid action items
            "users_activate",
            "users_deactivate",
            "users_lock",
            "users_unlock",
            "users_clone",
            "users_view",
            "users_edit",
            "users_more",
            "users_resend_invite",

            "users_new_user",

            //users list column names
            "users_user_col",
            "users_products_col",
            "users_properties_col",
            "users_last_login_col",
            "users_status_col",
            "users_action_col",

            //more filters
            "users_more_filters",
            "users_filter_user",
            "users_user_type",
            "users_acct_state",
            "users_lock_state",
            "users_all",
            "users_all_products",
            "users_all_properties",

            //more filters buttons
            "users_apply",
            "users_reset",
            "users_cancel",

            //user account status
            "users_active",
            "users_disabled",
            "users_expired",
            "users_pending",

            //users lock status
            "users_locked",
            "users_unlocked",

            //user types
            // "users_type_reguser",
            // "users_type_sysad",
            // "users_type_reguser_noemail",
            // "user_type_employee",
            "options.userTypeId.401",
            "options.userTypeId.402",
            "options.userTypeId.403",
            "options.userTypeId.404",
            "options.userTypeId.405",
            //"users_type_external",

            //user invite notification
            "users_invite_sent_title",
            "users_invite_sent_msg",

            //user status notification
            "user_status_error_msg",

            //file export
            "export.fileName",
            "text.exportToCSV",
            "text.exportToExcel",
            "text.exportToPDF"


        ];

        appLangKeys.app("people.user-list").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
