//  Bind Renters Insurance Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(15)) {
            productAccess.register({
                model: model,
                key: "soln204",
                product: "15"
            });
        }
    }

    angular
        .module("settings")
        .run(["rentersInsuranceDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
