//  Bind ILM Lead Management Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(40)) {
            productAccess.register({
                model: model,
                key: "soln308",
                product: "40"
            });
        }
    }

    angular
        .module("settings")
        .run(["ilmLeadManagementDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
