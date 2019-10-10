//  Job Title Options

(function (angular) {
    "use strict";

    function jobTitleOptions(ENV, optionsCache) {
        var defaultOptions,
            model = optionsCache(ENV.landingAPI + "api/roletypes");

        defaultOptions = [
            {
                name: "Select Job Title",
                partyRoleTypeId: ""
            }
        ];

        model.setReqData({
            roleTypeName: "person role"
        });

        return model.setDefaultOptions(defaultOptions);
    }

    angular
        .module("settings")
        .factory("jobTitleOptions", [
            "ENV",
            "optionsCache",
            jobTitleOptions
        ]);
})(angular);
