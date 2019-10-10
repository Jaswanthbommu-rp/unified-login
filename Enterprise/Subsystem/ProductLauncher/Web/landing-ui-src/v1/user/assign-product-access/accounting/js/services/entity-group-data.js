//  Accounting Entity Group Service

(function(angular) {
    "use strict";

    function AEntityGroupSvc($resource) {
        var url = "api/settings/users/accountingentitygroup";
        return $resource(url);
    }

    angular
        .module("settings")
        .factory("AEntityGroupSvc", ["$resource", AEntityGroupSvc]);
})(angular);
