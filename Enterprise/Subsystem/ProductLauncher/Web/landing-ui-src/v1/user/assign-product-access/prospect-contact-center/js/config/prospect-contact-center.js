//  Bind Prospect Contact Center Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        // productAccess.register({
        //     model: model,
        //     key: "soln302"
        // });
    }

    angular
        .module("settings")
        .run(["prospectContactCenterDataModel", "assignProductAccessModel", config]);
})(angular);
