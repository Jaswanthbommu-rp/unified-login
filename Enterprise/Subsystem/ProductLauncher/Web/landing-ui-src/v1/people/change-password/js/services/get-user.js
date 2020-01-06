//  Get User Service

(function(angular) {
    "use strict";


    function factory(ENV, $resource) {
        var params,
            url = ENV.landingAPI + "api/credential/getUser";


        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("getUserSvc", [
            "ENV",
            "$resource",            
            factory
        ]);
})(angular);