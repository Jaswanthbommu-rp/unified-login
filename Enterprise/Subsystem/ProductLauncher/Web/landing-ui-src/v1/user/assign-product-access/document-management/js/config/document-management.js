//  Bind Document Management Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln110"
        });
    }

    angular
        .module("settings")
        .run(["documentManagementDataModel", "assignProductAccessModel", config]);
})(angular);
