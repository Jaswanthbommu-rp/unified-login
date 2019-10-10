//  Bind OnSite Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln307"
        });
    }

    angular
        .module("settings")
        .run(["onSiteDataModel", "assignProductAccessModel", config]);
})(angular);
