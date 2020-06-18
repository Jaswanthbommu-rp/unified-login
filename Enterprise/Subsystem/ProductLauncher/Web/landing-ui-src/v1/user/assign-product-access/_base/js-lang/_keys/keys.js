(function () {
    "use strict";

    function config(appLangKeys) {
        var keys = [
            "panelName.accounting",
            "panelName.clientPortal",
            "panelName.lead2Lease",
            "panelName.marketingCenter",
            "panelName.on-site",
            "panelName.onesite",
            "panelName.prospectContactCenter",
            "panelName.spendManagement",
            "panelName.vendorCompliance",
            "panelName.residentPortals",
            "panelName.rentersInsurance",
            "panelName.selfProvisioningPortal",
            "panelName.utilityManagement",
            "panelName.unifiedAmenities",
            "panelName.businessintelligence",
            "panelName.investmentanalytics",
            "panelName.axiometrics",
            "panelName.performancenalytics",
            "panelName.revenuemanagement",
            "panelName.lro",
            "panelName.amenityoptimization",
            "panelName.airevenuemanagement",
            "panelName.rentcontrol",
            "panelName.ilmleadmanagement",
            "panelName.ilmleadanalytics",
            "panelName.portfolioManagement",
            "panelName.easylms",
            "panelName.clickPay",
            "panelName.depositAlternative",
            "panelName.renovationManager",
            "panelName.comingSoon"
        ];

        appLangKeys.app("assignProductAccess").set(keys);
    }

    angular
        .module("settings")
        .config(["appLangKeysProvider", config]);
})();
