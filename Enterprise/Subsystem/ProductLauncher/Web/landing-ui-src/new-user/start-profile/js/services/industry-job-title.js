// Start Profile Options - Industry Job Title

(function (angular) {
    "use strict";

    function industryJobTitleSvc($resource, ENV) {
        var url = ENV.landingAPI + "api/roleTypes",
            actions = {
                getList: {
                    method: "GET"
                }
            },

            paramsData = {
                roleTypeName: "person role"
            };

        return $resource(url, paramsData, actions);
    }

    angular
        .module("new-user")
        .service("industryJobTitleSvc", [
            "$resource",
            "ENV",
            industryJobTitleSvc
        ]);
})(angular);