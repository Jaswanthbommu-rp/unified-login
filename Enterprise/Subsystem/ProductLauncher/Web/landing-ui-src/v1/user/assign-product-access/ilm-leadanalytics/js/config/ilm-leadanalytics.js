//  Bind ILM Leasing Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
       if (!templateModel.isProductExists(41)) {
            productAccess.register({
                model: model,
                key: "soln309",
                product: "41"
            });
        }
    }

    angular
        .module("settings")
        .run(["ilmLeadAnalyticsDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
