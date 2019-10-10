//  Bind Click Pay Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln206"
        });
    }

    angular
        .module("settings")
        .run(["clickPayProductAccessModel", "assignProductAccessModel", config]);
})(angular);
