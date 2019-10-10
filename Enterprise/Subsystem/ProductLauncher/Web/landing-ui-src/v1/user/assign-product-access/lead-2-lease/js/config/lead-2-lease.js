//  Bind Lead 2 Lease Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln305"
        });
    }

    angular
        .module("settings")
        .run(["lead2LeaseDataModel", "assignProductAccessModel", config]);
})(angular);
