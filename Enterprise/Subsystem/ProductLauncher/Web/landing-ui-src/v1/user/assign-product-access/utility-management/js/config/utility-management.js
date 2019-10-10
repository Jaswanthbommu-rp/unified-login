//  Bind Utility Management Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln205"
        });
    }

    angular
        .module("settings")
        .run(["utilityManagementDataModel", "assignProductAccessModel", config]);
})(angular);
