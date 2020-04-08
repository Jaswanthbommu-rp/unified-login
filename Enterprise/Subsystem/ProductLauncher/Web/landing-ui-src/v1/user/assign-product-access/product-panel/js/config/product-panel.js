//  Bind  Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (templateModel.isProductExists(1)) {
            productAccess.register({
                model: model,
                key: "soln101",
                product: "1"
            });
        }

        if (templateModel.isProductExists(10)) {
            productAccess.register({
                model: model,
                key: "soln302",
                product: "10"
            });
        }

        if (templateModel.isProductExists(14)) {
            productAccess.register({
                model: model,
                key: "soln501",
                product: "14"
            });
        }

        if (templateModel.isProductExists(9)) {
            productAccess.register({
                model: model,
                key: "soln303",
                product: "9"
            });
        }

        if (templateModel.isProductExists(3)) {
            productAccess.register({
                model: model,
                key: "soln503",
                product: "3"
            });
        }


        logc("productAccess", productAccess);
    }

    angular
        .module("settings")
        .run(["productPanelDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
