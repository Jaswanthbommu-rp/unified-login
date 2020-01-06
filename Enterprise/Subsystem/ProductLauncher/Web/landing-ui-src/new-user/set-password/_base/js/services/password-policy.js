//  Get Change Password Policy Service

(function(angular) {
    "use strict";


    function factory(ENV, $resource) {
        var params,
            url = ENV.landingAPI + "api/passwordpolicies/:PartyId";


        return $resource(url, params);
    }

    angular
        .module("new-user")
        .factory("PasswordPolicySvc", [
            "ENV",
            "$resource",            
            factory
        ]);
})(angular);