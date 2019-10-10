(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("dashboard");

        bundle.set({

            dashboard_manage_profile: "Manage Profile",

            dashboard_properties: "Properties",
            dashboard_products: "Products",
            dashboard_roles: "Roles",

            dashboard_achievements: "Training & Achievements",
            dashboard_my_products: "My Products",

            dashboard_resources: "Resources",
            dashboard_client_portal: "Client Portal",
            dashboard_billing_portal: "Billing Portal",
            dashboard_prod_learning_portal: "Product Learning Portal",
            dashboard_research_application: "Master Data Management",
            dashboard_product_updates: "Product Updates",
            dashboard_support_tool: "Support Tool",
            dashboard_settings_console: "Settings Management Console",

            dashboard_view_all: "View all",
            dashboard_launch: "Launch",

            dashboard_error: "Unable to retrieve details. Please refresh page to try again.",

            dashboard_learn_more: "Learn More",


        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
