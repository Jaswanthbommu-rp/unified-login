//  Bind res-app Model

(function(angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln601"
        });
    }

    angular
        .module("settings")
        .run(["resAppDataModel", "assignProductAccessModel", config]);
})(angular);