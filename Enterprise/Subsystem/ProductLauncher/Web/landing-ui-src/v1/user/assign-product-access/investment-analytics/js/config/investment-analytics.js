//  Bind Investment Analytics Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(31)) {
            productAccess.register({
                model: model,
                key: "soln404",
                product: "31"
            });
        }
    }

    angular
        .module("settings")
        .run(["investmentAnalyticsDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
