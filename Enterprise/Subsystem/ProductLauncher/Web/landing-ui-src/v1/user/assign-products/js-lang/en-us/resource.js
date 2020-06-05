// Assign Products Resource

(function () {
    "use strict";

    function config(appLangBundle) {
        var bundle = appLangBundle.lang("en-us").app("assignProducts");

        bundle.set({
            "text.title": "Products",

            "text.familyTitle.100": "Property Management",
            "text.familyTitle.200": "Resident Services",
            "text.familyTitle.300": "Lease Management",
            "text.familyTitle.400": "Asset Optimization",
            "text.familyTitle.500": "Administration",
            "text.familyTitle.600": "Internal Tools",
            "text.familyTitle.700": "Asset & Investment Management",

            "text.solnTitle.101": "OneSite",
            "text.solnTitle.102": "Financial Suite",
            "text.solnTitle.104": "Spend Management",
            "text.solnTitle.105": "Vendor Credentialing",
            "text.solnTitle.107": "Unified Amenities",
            "text.solnTitle.110": "Document Director",
            "text.solnTitle.111": "EasyLMS",
            "text.solnTitle.112": "Renovation Manager",
            "text.solnTitle.201": "Resident Portals",
            "text.solnTitle.204": "Renters Insurance",
            "text.solnTitle.205": "Utility Management",
            "text.solnTitle.206": "Payments",
            "text.solnTitle.302": "Prospect Contact Center",
            "text.solnTitle.303": "Marketing Center",
            "text.solnTitle.305": "Lead2Lease",
            "text.solnTitle.306": "Omni Channel",
            "text.solnTitle.307": "On-Site",
            "text.solnTitle.308": "ILM Lead Management",
            "text.solnTitle.309": "ILM Leasing Analytics",
            "text.solnTitle.310": "Deposit Alternative",
            "text.solnTitle.401": "YieldStar",
            "text.solnTitle.402": "Business Intelligence",
            "text.solnTitle.403": "Performance Analytics",
            "text.solnTitle.404": "Investment Analytics",
            "text.solnTitle.406": "Axiometrics",
            "text.solnTitle.407": "LRO",
            "text.solnTitle.408": "Amenity Optimization",
            "text.solnTitle.409": "AI Revenue Management",
            "text.solnTitle.410": "Rent Control",
            "text.solnTitle.501": "Client Portal",
            "text.solnTitle.503": "Unified Platform",
            "text.solnTitle.504": "Self-Provisioning Portal",
            "text.solnTitle.505": "Integration Marketplace",
            "text.solnTitle.601": "Master Data Management",
            "text.solnTitle.701": "Portfolio Management"
        });

        bundle.test();
    }

    angular
        .module("settings")
        .config(["appLangBundleProvider", config]);
})();
