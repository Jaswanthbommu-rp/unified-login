//  Bind Client Portal Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        // productAccess.register({
        //     model: model,
        //     key: "soln501"
        // });

    }

    angular
        .module("settings")
        .run(["clientPortalDataModel", "assignProductAccessModel", config]);
})(angular);
