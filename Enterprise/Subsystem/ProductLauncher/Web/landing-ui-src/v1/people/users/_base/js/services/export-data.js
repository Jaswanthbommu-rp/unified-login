//  Export Data Service

(function (angular) {
    // "use strict";

    function exportData($resource, ENV) {
        var url, params;

        url = ENV.landingAPI + "api/persons/export";

        var actions = {
            getList: {
                method: "GET",
                cancellable: true
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .factory("exportData", ["$resource", "ENV", exportData]);
})(angular);

