(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("dashboard");

        bundle.set({

            "label.dashboard.manageProfile": "Manage Profile",
            "label.dashboard.properties": "Properties",
            "label.dashboard.products": "Products",
            "label.dashboard.roles": "Roles",
            "label.dashboard.achievements": "Training & Achievements (Coming Soon!)",
            "label.dashboard.myProducts": "My Products",
            "label.dashboard.resources": "Resources",
            "label.dashboard.clientPortal": "Client Portal",
            "label.dashboard.billingPortal": "Billing Portal",
            "label.dashboard.prodLearningPortal": "Product Learning Portal",
            "label.dashboard.viewAll": "View All Products",
            "label.dashboard.launch": "Launch",
            "label.dashboard.learnMore": "Learn More",
            "label.dashboard.welcomeBack": "Welcome back",
            "label.dashboard.not": "Not",

            "errorMsgs.dashboard.noDetails": "Unable to retrieve details. Please refresh page to try again.",

            "userResources.text.prod14": "Client Portal",
            // "userResources.text.prod1": "OneSite Conversions",
            "userResources.text.prod19": "Product Learning Portal",
            "userResources.text.prod21": "L&R Conversion Tool",
            "userResources.text.prod24": "Master Data Management",
            "userResources.text.prod25": "Self-Provisioning Portal",
            "userResources.text.prod26": "Unified Amenities",
            "userResources.text.prod27": "Migration Tool",
            "userResources.text.prod28": "Product Updates",
            "userResources.text.prod35": "Support Tool",
            "userResources.text.prod39": "Integration Marketplace",
            "userResources.text.prod40": "Intelligent Lead Management",
            "userResources.text.prod41": "Intelligent Lead Management-Leasing Analytics",
            //skipped 42 extra Sales Force call
            "userResources.text.prod43": "Settings Management",
            "userResources.text.prod45": "CIMPL"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
