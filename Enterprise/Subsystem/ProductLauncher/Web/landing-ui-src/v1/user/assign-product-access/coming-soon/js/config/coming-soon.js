//  Bind Coming Soon Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "default"
        });
    }

    angular
        .module("settings")
        .run(["comingSoonProductAccessModel", "assignProductAccessModel", config]);
})(angular);
