// Start Profile Options - Phone Types

(function (angular) {
    "use strict";

    function phoneTypeSvc($resource, ENV) {

        var url = ENV.landingAPI + "api/contactMechanismUsageTypes",
            actions = {
                getList: {
                    method: "GET"
                }
            },

            paramsData = {
                contactMechanismUsageTypeName: "phone type"
            };

        return $resource(url, paramsData, actions);
    }

    angular
        .module("new-user")
        .service("phoneTypeTitleSvc", [
            "$resource",
            "ENV",
            phoneTypeSvc
        ]);
})(angular);