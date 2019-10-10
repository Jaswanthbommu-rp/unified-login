//  Bind Unified Amenities Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln107"
        });
    }

    angular
        .module("settings")
        .run(["unifiedAmenitiesProductAccessModel", "assignProductAccessModel", config]);
})(angular);
