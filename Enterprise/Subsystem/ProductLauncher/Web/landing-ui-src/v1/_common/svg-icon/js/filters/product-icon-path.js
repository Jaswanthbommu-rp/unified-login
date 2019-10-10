// Product Icon Filter

(function (angular) {
    "use strict";

    function filter(cdnVer) {
        var icons = {
            "PROD1": "products/onesite", //  OneSite
            "PROD2": "", //  UnifiedUI
            "PROD4": "products/asset-optimization", //  Asset Optimization
            "PROD5": "products/propertyware", //  Propertyware
            "PROD6": "products/lead2lease", //  Lead2Lease
            "PROD7": "products/revenue-management", //  Revenue Management (Yieldstar)
            "PROD8": "products/realpage-accounting", //  RealPage Accounting - Financial Suite
            "PROD9": "products/marketing-center", //  Marketing Center
            "PROD10": "products/prospect-contact-center", //  Prospect Contact Center
            "PROD11": "", //  Social
            "PROD12": "", //  Ops Bid
            "PROD13": "products/spend-management", //  Spend Management
            "RES14": "resources/client-portal", //  Client Portal
            "PROD15": "products/renters-insurance", //  Renters Insurance
            "PROD16": "products/vendor-services", //  Vendor Services
            "PROD17": "products/resident-portals", //  Resident Portal
            "PROD18": "products/utility-management", //  Utility Management
            "RES19": "resources/learning-portal", //  Product Learning Portal
            "PROD20": "products/realpage-document-management", //  RealPage Document Management
            "RES21": "resources/leasing-and-rent-conversion-tool", //  Leasing & Rents Conversion Tool
            "PROD22": "products/omnichannel", //  OmniChannel
            "PROD23": "products/on-site", //  OnSite
            "RES24": "resources/research-application", //  Research Application - Blackbook
            "RES25": "resources/self-provisioning-portal", //  self-provisioning portal
            "PROD26": "products/unified-amenities", //  Unified Amenities
            "RES27": "resources/migration-tool", //  Migration Tool
            "RES28": "resources/product-updates", //  Product Updates

            "PROD29": "products/business-intelligence", //  Business Intelligence
            "PROD30": "products/performance-analytics", //  Performance Analytics
            "PROD31": "products/investment-analytics", //  Investment Analytics
            "PROD32": "products/revenue-management", //  Revenue Management
            "PROD33": "products/axiometrics", //  Axiometrics

            "RES35": "resources/support-tool", // support tool (resource)

            "PROD36": "products/easy-lms", // easyLMS
            "PROD37": "products/property-photos", // Property Photos
            "PROD38": "products/vendor-marketplace", // Vendor Marketplace
            "RES39": "resources/integration-marketplace", // Integration Marketplace
            "PROD40": "products/intelligent-lead-management", // ILM Lead Management
            "PROD41": "products/ilm-leasing-analytics", // ILM Leasing Analytics
            // skipped PROD42 for an extra Sales Force call
            "RES43": "resources/settings-management", // Settings Management Tool (resource)
            "PROD44": "products/portfolio-asset-management", // Portfolio Management
            "RES45": "resources/cimpl", // CIMPL

            "FAM100": "families/property-management",
            "FAM200": "families/resident-services",
            "FAM300": "families/lease-management",
            "FAM400": "families/asset-optimization",
            "FAM700": "families/asset-and-investment-management"
        };

        return function (guid) {
            guid = guid.toUpperCase();

            if (!icons[guid]) {
                logc("Icon for guid " + guid + " is undefined!");
                return "";
            }

            return "https://cdn.realpage.com/images/" + icons[guid] + ".svg";
        };
    }

    angular
        .module("settings")
        .filter("productIconPath", ["cdnVer", filter]);
})(angular);