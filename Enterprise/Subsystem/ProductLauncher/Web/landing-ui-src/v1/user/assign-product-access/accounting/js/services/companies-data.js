//  Accounting companies Service

(function(angular) {
    "use strict";

    function ACompaniesSvc($resource, ENV) {
        var url, params, actions;
        url = ENV.landingAPI + "api/products/onesiteaccounting/user/companies";

        params = {
            editorPersonaId: "@editorPersonalID",
            userPersonaId: "@userPersonalID",
            assignedOnly: "false"
        };

        actions = {
            get: {
                method: "GET",
                cancellable : true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("ACompaniesSvc", ["$resource", "ENV", ACompaniesSvc]);
})(angular);
