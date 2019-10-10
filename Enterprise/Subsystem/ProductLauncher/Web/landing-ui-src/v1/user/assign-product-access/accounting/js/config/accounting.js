//  Bind Accounting Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln102"
        });
    }

    angular
        .module("settings")
        .run(["AccountingDataModel", "assignProductAccessModel", config]);
})(angular);
