//  company Grid Data Service


(function(angular) {
    "use strict";

    function factory($resource, $window, ENV) {               
        
        var params,
            url = ENV.landingAPI + "api/employeeaccess/users";


        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("empAccessUserSvc", ["$resource", "$window", "ENV", factory]);
})(angular);



