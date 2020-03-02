//  Bind  Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {

        productAccess.register({
            model: model,
            key: "soln000"
        });
        logc("productAccess",productAccess);
    }

    angular
        .module("settings")
        .run(["productPanelDataModel", "assignProductAccessModel", config]);
})(angular);
