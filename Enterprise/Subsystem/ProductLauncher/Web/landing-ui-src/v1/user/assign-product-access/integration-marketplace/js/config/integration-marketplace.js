//  Bind integMkt Model

(function(angular) {
    "use strict";

    function config(model, productAccess, templateModel) {
    if (!templateModel.isProductExists(39)) {
            productAccess.register({
                model: model,
                key: "soln505",
                product: "39"
            });
        }
    }

    angular
        .module("settings")
        .run(["integMktDataModel", "assignProductAccessModel", "productTemplateModel", config]);
})(angular);