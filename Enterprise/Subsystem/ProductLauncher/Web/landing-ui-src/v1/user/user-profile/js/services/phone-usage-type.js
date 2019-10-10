//  Phone Label Options Service

(function (angular) {
    "use strict";

    function usageTypeOptions(ENV, optionsCache) {
        var defaultOptions,
            model = optionsCache(ENV.landingAPI + "api/contactmechanismusagetypes");

        defaultOptions = [
            {
                name: "Select Phone Type",
                contactMechanismUsageTypeId: null
            }
        ];

        model.setReqData({
            ContactMechanismUsageTypeName: "Phone Type"
        });

        return model.setDefaultOptions(defaultOptions);
    }

    angular
        .module("settings")
        .factory("usageTypeOptions", [
            "ENV",
            "optionsCache",
            usageTypeOptions
        ]);
})(angular);
