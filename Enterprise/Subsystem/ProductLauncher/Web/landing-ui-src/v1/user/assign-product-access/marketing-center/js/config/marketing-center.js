//  Bind Marketing Center Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(9)) {
            productAccess.register({
                model: model,
                key: "soln303",
                product: "9"
            });
        }

    }

    angular
        .module("settings")
        .run(["MarketingCenterDataModel", "assignProductAccessModel","productTemplateModel", config]);
})(angular);
