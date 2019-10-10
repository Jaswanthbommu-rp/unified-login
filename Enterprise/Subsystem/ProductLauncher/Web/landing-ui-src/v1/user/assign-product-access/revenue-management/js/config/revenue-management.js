//  Bind revenue management Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln401"
        });
    }

    angular
        .module("settings")
        .run(["revenueManagementDataModel", "assignProductAccessModel", config]);
})(angular);
