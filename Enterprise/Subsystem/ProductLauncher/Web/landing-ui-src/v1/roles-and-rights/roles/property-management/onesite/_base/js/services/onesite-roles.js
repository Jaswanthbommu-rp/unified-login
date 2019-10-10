//  Roles Grid Data Service


(function(angular) {
    "use strict";

    function factory($resource, $window, ENV) {
       
        var url, defaults, actions;

        actions = {};

        defaults = {};

        url = ENV.landingAPI + "api/products/onesite/role";

        return $resource(url, defaults, actions);        
    }

    angular
        .module("settings")
        .factory("onesiteRolesSvc", ["$resource", "$window", "ENV", factory]);
})(angular);