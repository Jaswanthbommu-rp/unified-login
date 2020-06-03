//  Bind Vendor Compliance Model

(function (angular) {
    "use strict";
        function config(model, productAccess, templateModel) {
            if (!templateModel.isProductExists(16)) {
                productAccess.register({
                    model: model,
                    key: "soln105",
                    product: "16"
                });
            }
        }
    
        angular
            .module("settings")
            .run(["vendorComplianceDataModel", "assignProductAccessModel", "productTemplateModel", config]);
    })(angular);
