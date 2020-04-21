//  Bind OnSite Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(23)) {
            productAccess.register({
                model: model,
                key: "soln307",
                product: "23"
            });
        }
    }

    angular
        .module("settings")
        .run(["onSiteDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
