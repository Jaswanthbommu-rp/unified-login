//  usermgmt Rights List Aside Service

(function(angular) {
    "use strict";

    function UserMgmtRightsListAsideSvc($resource, ENV) {
        var url, params, actions;

        url = ENV.landingAPI + "/api/products/unifiedlogin/role/rights";
      

        actions = {
            get: {
                method: "GET",
                cancellable: true
            }
        };

        return $resource(url, params, actions);
    }

    angular
        .module("settings")
        .factory("userMgmtRightsListAsideSvc", ["$resource", "ENV", UserMgmtRightsListAsideSvc]);
})(angular);
