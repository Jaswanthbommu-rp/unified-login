//  Bind axiometrics Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln406"
        });
    }

    angular
        .module("settings")
        .run(["axmDataModel", "assignProductAccessModel", config]);
})(angular);
