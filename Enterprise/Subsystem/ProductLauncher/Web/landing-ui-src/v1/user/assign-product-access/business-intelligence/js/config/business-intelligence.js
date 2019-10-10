//  Bind AO Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln402"
        });
    }

    angular
        .module("settings")
        .run(["businessIntelligenceDataModel", "assignProductAccessModel", config]);
})(angular);
