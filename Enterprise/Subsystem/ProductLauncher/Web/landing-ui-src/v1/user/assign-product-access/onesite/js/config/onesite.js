//  Bind OneSite Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln101"
        });
    }

    angular
        .module("settings")
        .run(["onesiteDataModel", "assignProductAccessModel", config]);
})(angular);
