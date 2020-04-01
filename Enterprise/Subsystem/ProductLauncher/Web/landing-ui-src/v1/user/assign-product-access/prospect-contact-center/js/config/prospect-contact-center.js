//  Bind Prospect Contact Center Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(10)) {
            productAccess.register({
                model: model,
                key: "soln302",
                product: "10"
            });
        }
    }

    angular
        .module("settings")
        .run(["prospectContactCenterDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
