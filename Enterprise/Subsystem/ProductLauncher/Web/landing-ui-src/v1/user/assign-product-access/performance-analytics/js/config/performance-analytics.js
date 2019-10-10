//  Bind performance Analytics Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln403"
        });
    }

    angular
        .module("settings")
        .run(["performanceAnalyticsDataModel", "assignProductAccessModel", config]);
})(angular);
