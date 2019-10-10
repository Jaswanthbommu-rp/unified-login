//  Bind Self-Provisioning Portal Model

(function (angular) {
    "use strict";

    function config(model, productAccess) {
        productAccess.register({
            model: model,
            key: "soln504"
        });
    }

    angular
        .module("settings")
        .run(["selfProvisioningPortalProductAccessModel", "assignProductAccessModel", config]);
})(angular);
