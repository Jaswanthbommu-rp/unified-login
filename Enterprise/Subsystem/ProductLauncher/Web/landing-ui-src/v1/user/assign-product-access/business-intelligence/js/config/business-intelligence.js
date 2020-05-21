//  Bind AO Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(29)) {
            productAccess.register({
                model: model,
                key: "soln402",
                product: "29"
            });
        }
    }

    angular
        .module("settings")
        .run(["businessIntelligenceDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
