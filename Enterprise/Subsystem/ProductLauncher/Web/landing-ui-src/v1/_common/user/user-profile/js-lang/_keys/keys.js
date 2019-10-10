(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "dashboard_manage_profile",

            "dashboard_properties",
            "dashboard_products",
            "dashboard_roles",

            "dashboard_achievements",
            "dashboard_my_products",

            "dashboard_resources",
            "dashboard_client_portal",
            "dashboard_billing_portal",
            "dashboard_prod_learning_portal",
            "dashboard_research_application",
            "dashboard_product_updates",
            "dashboard_support_tool",
            "dashboard_settings_console",

            "dashboard_view_all",
            "dashboard_launch",

            "dashboard_error",

            "dashboard_learn_more"

        ];

        appLangKeys.app("dashboard").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
