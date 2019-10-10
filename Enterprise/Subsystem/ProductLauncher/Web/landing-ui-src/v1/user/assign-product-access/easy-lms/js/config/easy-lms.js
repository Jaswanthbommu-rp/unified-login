//  Bind Coming Soon Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln111"
        });
    }

    angular
        .module("settings")
        .run(["easyLMSProductAccessModel", "assignProductAccessModel", config]);
})(angular);
