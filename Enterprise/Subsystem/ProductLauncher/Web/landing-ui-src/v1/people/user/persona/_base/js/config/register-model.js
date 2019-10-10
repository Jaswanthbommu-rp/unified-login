// Register Model

(function (angular) {
    "use strict";

    function config(overall, onesite, marketingCenter, accounting, spendManagement, vendorCompliance) {
    	overall.register("onesite", onesite);
    	overall.register("marketing-center", marketingCenter);
        overall.register("accounting", accounting);
        overall.register("spend-management", spendManagement);
        overall.register("vendor-compliance", vendorCompliance);
    }

    angular
        .module("settings")
        .run(["productsOverallDataModel", 
        	"OnesiteDataModel", 
        	"MarketingCenterDataModel", 
            "AccountingDataModel", 
            "SpendManagementDataModel",
            "VendorComplianceDataModel",
        	config]);
})(angular);
