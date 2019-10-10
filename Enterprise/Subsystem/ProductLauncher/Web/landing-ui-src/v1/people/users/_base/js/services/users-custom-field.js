//  Custom field Data Service


(function(angular) {
    "use strict";

    function factory($resource, $window, ENV) {               
        
        var params,
            url = ENV.landingAPI + "api/customfields";


        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("usersCustomFieldSvc", ["$resource", "$window", "ENV", factory]);
})(angular);



