//  Rights Data Service

(function(angular, undefined) {
    "use strict";

    function AcctNewRightsSvc(ENV, $resource, user) {
        var actions,
            defaults,
            resource,
            svc = this,
            url = ENV.landingAPI + "/api/products/onesiteaccounting/rights";

        defaults = {
            realPageId: user.getRealPageId()
        };

        actions = {
            save: {
                method: "PUT"
            }
        };

        resource = $resource(url, defaults, actions);

        svc.getData = function(realPageId) {
            return resource.get.apply(resource, arguments).$promise;
        };

        svc.save = function(realPageId, data) {
            return resource.save.apply(resource, arguments).$promise;
        };
    }

    angular
        .module("settings")
        .service("acctNewRightsSvc", [
            "ENV",
            "$resource",
            "userSessionModel",
            AcctNewRightsSvc
        ]);
})(angular);