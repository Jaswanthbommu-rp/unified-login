//  Bind Renters Insurance Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln204"
        });
    }

    angular
        .module("settings")
        .run(["rentersInsuranceDataModel", "assignProductAccessModel", config]);
})(angular);
