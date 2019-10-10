//  Bind Vendor Compliance Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln105"
        });
    }

    angular
        .module("settings")
        .run(["vendorComplianceDataModel", "assignProductAccessModel", config]);
})(angular);
