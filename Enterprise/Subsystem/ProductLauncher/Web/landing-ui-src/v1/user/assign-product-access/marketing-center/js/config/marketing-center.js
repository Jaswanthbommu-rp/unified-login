//  Bind Marketing Center Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln303"
        });
    }

    angular
        .module("settings")
        .run(["MarketingCenterDataModel", "assignProductAccessModel", config]);
})(angular);
