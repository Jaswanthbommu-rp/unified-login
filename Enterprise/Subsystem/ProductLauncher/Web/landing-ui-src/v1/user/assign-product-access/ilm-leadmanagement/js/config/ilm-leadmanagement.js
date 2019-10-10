//  Bind ILM Lead Management Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln308"
        });
    }

    angular
        .module("settings")
        .run(["ilmLeadManagementDataModel", "assignProductAccessModel", config]);
})(angular);
