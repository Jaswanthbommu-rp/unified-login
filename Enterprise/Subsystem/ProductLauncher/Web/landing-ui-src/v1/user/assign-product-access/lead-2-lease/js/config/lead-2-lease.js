//  Bind Lead 2 Lease Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(6)) {
            productAccess.register({
                model: model,
                key: "soln305",
                product: "6"
            });
        }
    }

    angular
        .module("settings")
        .run(["lead2LeaseDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
