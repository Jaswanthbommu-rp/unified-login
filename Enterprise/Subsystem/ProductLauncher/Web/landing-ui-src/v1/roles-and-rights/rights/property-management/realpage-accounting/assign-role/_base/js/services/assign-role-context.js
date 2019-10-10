//  assign Roles Context Service

(function(angular) {
    "use strict";

    function factory(rpDataShareModel) {
        return rpDataShareModel();
    }

    angular
        .module("settings")
        .factory("acctAssignRolesToRightsContext", ["rpDataShareModel", factory]);
})(angular);