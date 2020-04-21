//  Bind usermgmt Model

(function (angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
        if (!templateModel.isProductExists(3)) {
            productAccess.register({
                model: model,
                key: "soln503",
                product: "3"
            });
        }
    }

    angular
        .module("settings")
        .run(["userMgmtDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);
