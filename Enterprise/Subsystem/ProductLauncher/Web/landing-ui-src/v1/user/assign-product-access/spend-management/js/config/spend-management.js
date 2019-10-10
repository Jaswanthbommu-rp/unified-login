//  Bind Spend Management Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln104"
        });
    }

    angular
        .module("settings")
        .run(["spendManagementDataModel", "assignProductAccessModel", config]);
})(angular);
