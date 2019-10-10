(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [

            "label.dashboard.manageProfile",
            "label.dashboard.properties",
            "label.dashboard.products",
            "label.dashboard.roles",
            "label.dashboard.achievements",
            "label.dashboard.myProducts",
            "label.dashboard.resources",
            "label.dashboard.clientPortal",
            "label.dashboard.billingPortal",
            "label.dashboard.prodLearningPortal",
            "label.dashboard.viewAll",
            "label.dashboard.launch",
            "label.dashboard.learnMore",
            "label.dashboard.welcomeBack",
            "label.dashboard.not",

            "errorMsgs.dashboard.noDetails",

            "userResources.text.prod14",
            // "userResources.text.prod1",
            "userResources.text.prod19",
            "userResources.text.prod21",
            "userResources.text.prod24",
            "userResources.text.prod25",
            "userResources.text.prod26",
            "userResources.text.prod27",
            "userResources.text.prod28",
            "userResources.text.prod35",
            "userResources.text.prod39",
            "userResources.text.prod40",
            "userResources.text.prod41",
            //skipped 42 extra Sales Force call
            "userResources.text.prod43",
            "userResources.text.prod45"
        ];

        appLangKeys.app("dashboard").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
