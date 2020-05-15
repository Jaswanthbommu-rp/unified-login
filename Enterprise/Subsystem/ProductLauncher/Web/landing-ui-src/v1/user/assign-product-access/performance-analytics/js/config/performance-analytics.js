//  Bind performance Analytics Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(30)) {
            productAccess.register({
                model: model,
                key: "soln403",
                product: "30"
            });
        }
    }

    angular
        .module("settings")
        .run(["performanceAnalyticsDataModel", "assignProductAccessModel",  "productTemplateModel", config]);
})(angular);
