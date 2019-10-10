//  Marketing Center Propety Group Service

(function(angular) {
    "use strict";

    function MCPropertyGroupSvc($resource) {
        var url = "api/settings/users/marketingcenterpropertygroup"; //TODO: Link up to the correct API for property groups
        return $resource(url);
    }

    angular
        .module("settings")
        .factory("MCPropertyGroupSvc", [
        	"$resource", 
        	MCPropertyGroupSvc
        ]);
})(angular);
