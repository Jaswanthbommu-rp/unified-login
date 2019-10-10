//  Bind integMkt Model

(function(angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln505"
        });
    }

    angular
        .module("settings")
        .run(["integMktDataModel", "assignProductAccessModel", config]);
})(angular);