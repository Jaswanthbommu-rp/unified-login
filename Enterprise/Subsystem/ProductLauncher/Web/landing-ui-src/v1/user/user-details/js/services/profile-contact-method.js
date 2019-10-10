// Profile Options - Contact Method

(function (angular) {
    "use strict";

    function contactMethodSvc($resource, ENV) {
        var url = ENV.landingAPI + "api/preferredcontactmethods",
            actions = {
                getList: {
                    method: "GET"
                }
            },

            paramsData = {};

        return $resource(url, paramsData, actions);
    }

    angular
        .module("settings")
        .service("contactMethodSvc", [
            "$resource",
            "ENV",
            contactMethodSvc
        ]);
})(angular);
