//  Bind OneSite Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(1)) {
            logc("templateModel", templateModel);
            productAccess.register({
                model: model,
                key: "soln101",
                product: "1"
            });
        }
    }

    angular
        .module("settings")
        .run(["onesiteDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
