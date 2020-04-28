//  Bind Resident Portals Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
         if (!templateModel.isProductExists(17)) {
            productAccess.register({
                model: model,
                key: "soln201",
                product: "17"
            });
        }
    }

    angular
        .module("settings")
        .run(["residentPortalsDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
