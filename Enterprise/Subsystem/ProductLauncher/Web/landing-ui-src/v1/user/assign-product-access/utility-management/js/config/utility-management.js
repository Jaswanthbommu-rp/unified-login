//  Bind Utility Management Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(18)){
            productAccess.register({
                model: model,
                key: "soln205",
                product: "18"
            });
        }
    }

    angular
        .module("settings")
        .run(["utilityManagementDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
