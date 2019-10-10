//  company Grid Data Service


(function(angular) {
    "use strict";

    function factory($resource, $window, ENV) {               
        
        var params,
            url = ENV.landingAPI + "api/employeeaccess/companies";


        return $resource(url, params);
    }

    angular
        .module("settings")
        .factory("empAccessCompSvc", ["$resource", "$window", "ENV", factory]);
})(angular);



