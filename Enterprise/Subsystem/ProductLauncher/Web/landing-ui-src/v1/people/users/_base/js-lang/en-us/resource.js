(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("people.user-list");

        bundle.set({

            //select all dropdown
            users_select_visible: "Select visible",
            users_select_all: "Select All",

            //grid action items
            users_activate: "Activate",
            users_deactivate: "Deactivate",
            users_lock: "Lock",
            users_unlock: "Unlock",
            users_clone: "Clone",
            users_view: "View",
            users_edit: "Edit",
            users_more: "More",
            users_resend_invite: "Resend Invite",

            users_new_user: "New User",

            //users list column names
            users_user_col: "Name/Username",
            users_products_col: "Products",
            users_properties_col: "Properties",
            users_last_login_col: "Last Login",
            users_status_col: "Status",
            users_action_col: "Action",

            //more filters
            users_more_filters: "More Filters",
            users_filter_user: "Find a person...",
            users_user_type: "User Type",
            users_acct_state: "Status",
            users_lock_state: "Lock/Unlock",
            users_all: "All",
            users_all_products: "All products",
            users_all_properties: "All properties",

            //more filters buttons
            users_apply: "Apply",
            users_reset: "Clear",
            users_cancel: "Cancel",

            //user account status
            users_active: "Active",
            users_disabled: "Deactivated",
            users_expired: "Expired",
            users_pending: "Pending",

            //users lock status
            users_locked: "Locked",
            users_unlocked: "Unlocked",

            //user types
            // users_type_reguser: "Regular User",
            // users_type_sysad: "RealPage System Administrator",
            // users_type_reguser_noemail: "Regular User (no email)",
            // user_type_employee : "RealPage Employee",
            "options.userTypeId.401": "Regular User",
            "options.userTypeId.402": "RealPage System Administrator",
            "options.userTypeId.404": "Regular User (no email)",
            "options.userTypeId.403": "RealPage Employee",
            "options.userTypeId.405": "External User",
            //users_type_external : "External User",

            //user invite notification
            users_invite_sent_title: "User Invitiations Sent",
            users_invite_sent_msg: "Invitiation emails sent successfully to the selected users.",

            //user status notification
            user_status_error_msg: "The Request Failed",

            //file export
            "export.fileName": "userlist",
            "text.exportToCSV": "Export to CSV",
            "text.exportToExcel": "Export to Excel",
            "text.exportToPDF": "Export to PDF",


        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
