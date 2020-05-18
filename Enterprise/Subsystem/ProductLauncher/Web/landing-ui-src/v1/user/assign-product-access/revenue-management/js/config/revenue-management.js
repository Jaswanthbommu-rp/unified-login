//  Bind revenue management Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(32)) {
            productAccess.register({
                model: model,
                key: "soln401",
                product: "32"
            });
        }
    }

    angular
        .module("settings")
        .run(["revenueManagementDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
