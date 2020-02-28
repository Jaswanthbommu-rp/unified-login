//  assign properties Context Service

(function(angular) {
    "use strict";

    function factory(rpDataShareModel) {
        return rpDataShareModel();
    }

    angular
        .module("settings")
        .factory("pmAssignEntitiesContext", ["rpDataShareModel", factory]);
})(angular);
