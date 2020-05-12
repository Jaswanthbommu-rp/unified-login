//  Bind ILM Leasing Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln309"
        });
    }

    angular
        .module("settings")
        .run(["ilmLeadAnalyticsDataModel", "assignProductAccessModel", config]);
})(angular);
