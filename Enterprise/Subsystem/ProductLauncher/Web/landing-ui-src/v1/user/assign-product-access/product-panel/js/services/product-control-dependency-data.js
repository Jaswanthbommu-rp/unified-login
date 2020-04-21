// Controls Service

(function(angular) {
    "use strict";

    function ProductControlDependencySvc($resource, ENV) {
        var url, params,actions;
        //url = "https://ulapi-dev.realpage.com/v2/UserMgmt/ProductAccess";
        url =  ENV.landingAPI +"apicore/v2/UserMgmt/ControlDependency";

        params = {
            controlId: "@controlId"
        };

         actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("productControlDependencySvc", [
            "$resource",
            "ENV",
            ProductControlDependencySvc
        ]);
})(angular);
