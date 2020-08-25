(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("assignProductAccess");

        bundle.set({
            "panelName.accounting": "Financial Suite Product Access",
            "panelName.clientPortal": "Client Portal Product Access",
            "panelName.documentManagement": "Document Director Product Access",
            "panelName.lead2Lease": "Lead2Lease Product Access",
            "panelName.marketingCenter": "Marketing Center Product Access",
            "panelName.on-site": "On-Site Product Access",
            "panelName.onesite": "OneSite Product Access",
            "panelName.prospectContactCenter": "Prospect Contact Center Product Access",
            "panelName.spendManagement": "Spend Management Product Access",
            "panelName.vendorCompliance": "Vendor Credentialing Product Access",
            "panelName.residentPortals": "Resident Portals Product Access",
            "panelName.rentersInsurance": "Renters Insurance Product Access",
            "panelName.selfProvisioningPortal": "Self-Provisioning Portal",
            "panelName.utilityManagement": "Utility Management Product Access",
            "panelName.unifiedAmenities": "Unified Amenities Product Access",
            "panelName.businessintelligence": "Business Intelligence Product Access",
            "panelName.investmentanalytics": "Investment Analytics Product Access",
            "panelName.axiometrics": "Axiometrics Product Access",
            "panelName.performancenalytics": "Performance Analytics Product Access",
            "panelName.revenuemanagement": "YieldStar Product Access",
            "panelName.lro": "LRO Product Access",
            "panelName.amenityoptimization": "Amenity Optimization Product Access",
            "panelName.airevenuemanagement": "AI Revenue Management Product Access",
            "panelName.rentcontrol": "Rent Control Product Access",
            "panelName.ilmleadmanagement": "ILM Lead Management Product Access",
            "panelName.ilmleadanalytics": "ILM Leasing Analytics Product Access",
            "panelName.portfolioManagement": "Portfolio Management Product Access",
            "panelName.easylms": "EasyLMS Product Access",
            "panelName.clickPay" : "Payments Product Access",
            "panelName.depositAlternative" : "Deposit Alternative Product Access",
            "panelName.renovationManager" : "Renovation Manager Product Access",
            "panelName.intelligentBuilding" : "Waste Management Solution Product Access",
            "panelName.comingSoon": "Coming Soon",

            "panelError.generic": "There was a problem getting information for the product"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
