//  Bind Investment Analytics Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln404"
        });
    }

    angular
        .module("settings")
        .run(["investmentAnalyticsDataModel", "assignProductAccessModel", config]);
})(angular);
