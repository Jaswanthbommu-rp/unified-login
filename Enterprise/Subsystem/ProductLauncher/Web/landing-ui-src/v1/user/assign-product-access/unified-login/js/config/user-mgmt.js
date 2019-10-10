//  Bind usermgmt Model

(function(angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln503"
        });
    }

    angular
        .module("settings")
        .run(["userMgmtDataModel", "assignProductAccessModel", config]);
})(angular);