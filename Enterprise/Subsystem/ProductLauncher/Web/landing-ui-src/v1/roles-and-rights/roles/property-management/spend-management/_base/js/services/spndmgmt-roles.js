//  Roles Grid Data Service


(function(angular) {
    "use strict";

    function factory($resource, $window, ENV) {
       
        var url, defaults, actions;

        actions = {};

        defaults = {};

        url = ENV.landingAPI + "api/products/ops/rolescount";

        return $resource(url, defaults, actions);        
    }

    angular
        .module("settings")
        .factory("spndmgmtRolesSvc", ["$resource", "$window", "ENV", factory]);
})(angular);