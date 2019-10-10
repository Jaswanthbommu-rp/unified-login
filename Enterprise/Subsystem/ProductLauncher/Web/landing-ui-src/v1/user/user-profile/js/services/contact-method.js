//  Contact Method Service

(function (angular, undefined) {
    "use strict";

    function factory(ENV, optionsCache) {
        var defaultOptions,
            model = optionsCache(ENV.landingAPI + "api/preferredcontactmethods");

        defaultOptions = [
            {
                name: "Select Contact Method",
                preferredContactMethodId: ""
            }
        ];

        model.setDefaultOptions(defaultOptions);

        return model;
    }

    angular
        .module("settings")
        .factory("contactMethodOptions", ["ENV", "optionsCache", factory]);
})(angular);
