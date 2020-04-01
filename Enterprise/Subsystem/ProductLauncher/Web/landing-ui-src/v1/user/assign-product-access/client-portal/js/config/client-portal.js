//  Bind Client Portal Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(14)) {
            productAccess.register({
                model: model,
                key: "soln501",
                product: "14"
            });
        }
    }

    angular
        .module("settings")
        .run(["clientPortalDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
