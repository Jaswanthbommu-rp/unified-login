//  Bind Deposit Alt Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln310"
        });
    }

    angular
        .module("settings")
        .run(["depositAlternativeProductAccessModel", "assignProductAccessModel", config]);
})(angular);
