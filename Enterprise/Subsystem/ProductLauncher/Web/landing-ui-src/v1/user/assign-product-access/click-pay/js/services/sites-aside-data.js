//  Click Pay sites List Aside Service

(function (angular) {
    "use strict";

    function CPSiteListAsideSvc($resource, ENV) {
        var url, params, actions;
               

        url = ENV.landingAPI + "api/products/organizations";
        params = {
            productType: "ClickPay",
            editorPersonaId: "@editorPersonaId",
            subjectPersonaId: "@userPersonaId",
            organizationRoleId: "@organizationRoleId",
            organizationType: "@organizationType",            
        };

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
        .factory("CPSiteListAsideSvc", ["$resource", "ENV", CPSiteListAsideSvc]);
})(angular);
