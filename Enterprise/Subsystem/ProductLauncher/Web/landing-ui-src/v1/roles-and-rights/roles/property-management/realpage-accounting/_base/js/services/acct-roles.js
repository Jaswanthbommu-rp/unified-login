//  Roles Grid Data Service


(function(angular) {
    "use strict";

    function factory($resource, $window, ENV) {
       
        var url, defaults, actions;

        actions = {};

        defaults = {};

        url = ENV.landingAPI + "api/products/onesiteaccounting/rolescount";

        return $resource(url, defaults, actions);        
    }

    angular
        .module("settings")
        .factory("acctRolesSvc", ["$resource", "$window", "ENV", factory]);
})(angular);