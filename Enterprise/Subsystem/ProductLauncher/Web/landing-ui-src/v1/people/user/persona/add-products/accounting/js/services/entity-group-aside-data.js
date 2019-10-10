//  Accounting Entity Group Aside Service

(function(angular) {
    "use strict";

    function AEntityGroupAsideSvc($resource) {
        var url = "api/settings/users/aentitygroupaside";
        return $resource(url);
    }

    angular
        .module("settings")
        .factory("AEntityGroupAsideSvc", ["$resource", AEntityGroupAsideSvc]);
})(angular);
