//  Bind  Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {

        productAccess.register({
            model: model,
            key: "soln101",
            product: "1"
        });

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

        productAccess.register({
            model: model,
            key: "soln303",
            product: "9"
        });

        logc("productAccess", productAccess);
    }

    angular
        .module("settings")
        .run(["productPanelDataModel", "assignProductAccessModel", config]);
})(angular);
