//  Product Templates Constant

(function(angular) {
    "use strict";

    // apa - assign product access

    var cnst = {
        "soln102": {
            name: "Financial Suite",
            templateUrl: "user/assign-product-access/accounting/templates/index.html"
        },

        "soln101": {
            name: "OneSite"
            //templateUrl: "user/assign-product-access/onesite/templates/index.html"
        },

        "soln104": {
            name: "Spend Management",
            templateUrl: "user/assign-product-access/spend-management/templates/index.html"
        },

        "soln105": {
            name: "Vendor Credentialing",
            templateUrl: "user/assign-product-access/vendor-compliance/templates/index.html"
        },

        "soln107": {
            name: "Unified Amenities",
            templateUrl: "user/assign-product-access/unified-amenities/templates/index.html"
        },

        "soln110": {
            name: "Document Director",
            templateUrl: "user/assign-product-access/document-management/templates/index.html"
        },

        "soln111": {
            name: "EasyLMS",
            templateUrl: "user/assign-product-access/easy-lms/templates/index.html"
        },

        "soln201": {
            name: "Resident Portals",
            templateUrl: "user/assign-product-access/resident-portal/templates/index.html"
        },

        "soln204": {
            name: "Renters Insurance",
            templateUrl: "user/assign-product-access/renters-insurance/templates/index.html"
        },

        "soln205": {
            name: "Utility Management",
            templateUrl: "user/assign-product-access/utility-management/templates/index.html"
        },

        "soln206": {
            name: "Payments",
            templateUrl: "user/assign-product-access/click-pay/templates/index.html"
        },

        "soln302": {
            name: "Prospect Contact Center"
            //templateUrl: "user/assign-product-access/prospect-contact-center/templates/index.html"
        },

        "soln303": {
            name: "Marketing Center"
            //templateUrl: "user/assign-product-access/marketing-center/templates/index.html"
        },

        "soln305": {
            name: "Lead 2 Lease",
            templateUrl: "user/assign-product-access/lead-2-lease/templates/index.html"
        },

        "soln307": {
            name: "On-Site",
            templateUrl: "user/assign-product-access/on-site/templates/index.html"
        },

        "soln308": {
            name: "ILM Lead Management",
            templateUrl: "user/assign-product-access/ilm-leadmanagement/templates/index.html"
        },

        "soln309": {
            name: "ILM Lead Analytics",
            templateUrl: "user/assign-product-access/ilm-leadanalytics/templates/index.html"
        },

        "soln310": {
            name: "Deposit Alternative",
            templateUrl: "user/assign-product-access/deposit-alternative/templates/index.html"
        },

        "soln401": {
            name: "YieldStar",
            templateUrl: "user/assign-product-access/revenue-management/templates/index.html"
        },

        "soln402": {
            name: "Business Intelligence",
            templateUrl: "user/assign-product-access/business-intelligence/templates/index.html"
        },

        "soln403": {
            name: "Performance Analytics",
            templateUrl: "user/assign-product-access/performance-analytics/templates/index.html"
        },

        "soln404": {
            name: "Investment Analytics",
            templateUrl: "user/assign-product-access/investment-analytics/templates/index.html"
        },

        // "soln406": {
        //     name: "Axiometrics",
        //     templateUrl: "user/assign-product-access/axiometrics/templates/index.html"
        // },

        // "soln501": {
        //     name: "Client Portal",
        //     templateUrl: "user/assign-product-access/client-portal/templates/index.html"
        // },
         "soln501": {
            name: "Client Portal"
        },

        "soln503": {
            name: "Unified Platform",
            templateUrl: "user/assign-product-access/unified-login/templates/index.html"
           //templateUrl: "user/assign-product-access/product-panel/templates/index.html"
        },

        "soln504": {
            name: "Self-Provisioning Portal",
            templateUrl: "user/assign-product-access/self-provisioning-portal/templates/index.html"
        },

        "soln505": {
            name: "Integration Marketplace",
            templateUrl: "user/assign-product-access/integration-marketplace/templates/index.html"
        },


        "soln601": {
            name: "Master Data Management",
            templateUrl: "user/assign-product-access/research-application/templates/index.html"
        },

        "soln701": {
            name: "Portfolio Management",
            templateUrl: "user/assign-product-access/portfolio-management/templates/index.html"
        },

        "soln000": {
            name: "Unified Platform",
           templateUrl: "user/assign-product-access/product-panel/templates/index.html"
        },

        "default": {
            name: "Coming Soon",
            templateUrl: "user/assign-product-access/coming-soon/templates/index.html"
        }
    };

    Object.freeze(cnst);

    angular
        .module("settings")
        .constant("productAccessData", cnst);
})(angular);
