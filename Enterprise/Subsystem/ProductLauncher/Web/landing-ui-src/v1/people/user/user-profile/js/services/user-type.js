//  User Type Options

(function (angular) {
    "use strict";

    function userTypeOptions(ENV, optionsCache) {
        var defaultOptions,
            model = optionsCache(ENV.landingAPI + "api/roleTypes");

        model.setReqData({
            roleTypeName: "user role"
        });

        return model;
    }

    angular
        .module("settings")
        .factory("userTypeOptions", [
            "ENV",
            "optionsCache",
            userTypeOptions
        ]);
})(angular);
