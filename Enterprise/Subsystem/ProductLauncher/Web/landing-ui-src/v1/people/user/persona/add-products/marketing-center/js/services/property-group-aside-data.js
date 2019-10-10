//  Marketing Center Propety Group Aside Service

(function(angular) {
    "use strict";

    function MCPropertyGroupAsideSvc($resource) {
        var url = "api/settings/users/mcpropertygroupaside";
        return $resource(url);
    }

    angular
        .module("settings")
        .factory("MCPropertyGroupAsideSvc", [
        	"$resource", 
        	MCPropertyGroupAsideSvc
        ]);
})(angular);
