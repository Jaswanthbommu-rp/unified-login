//  Bind Unified Amenities Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {

        if (!templateModel.isProductExists(26)) {
            productAccess.register({
                model: model,
                key: "soln107",
                product: "26"
            });
        }
    }

    angular
        .module("settings")
        .run(["unifiedAmenitiesProductAccessModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
