//  Bind Portfolio Management Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln701"
        });
    }

    angular
        .module("settings")
        .run(["portfolioManagementDataModel", "assignProductAccessModel", config]);
})(angular);
