//  Bind Resident Portals Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln201"
        });
    }

    angular
        .module("settings")
        .run(["residentPortalsDataModel", "assignProductAccessModel", config]);
})(angular);
