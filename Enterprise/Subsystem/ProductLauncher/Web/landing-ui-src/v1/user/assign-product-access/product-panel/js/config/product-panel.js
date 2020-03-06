//  Bind  Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {

        productAccess.register({
            model: model,
            key: "soln302",
            product: "10"
        });

        productAccess.register({
            model: model,
            key: "soln501",
            product: "14"
        });

        logc("productAccess", productAccess);
    }

    angular
        .module("settings")
        .run(["productPanelDataModel", "assignProductAccessModel", config]);
})(angular);
