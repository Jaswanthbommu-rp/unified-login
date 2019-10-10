//  Export Data Service

(function (angular) {
    // "use strict";

    function activityLogExportData($resource, ENV) {
        var url, params;

        url = ENV.loggingAPI + "api/exportactivitylog";

        var actions = {
            getActivity: {
                method: "POST",
                cancellable: true
            }
        };

        return $resource(url, {}, actions);
    }

    angular
        .module("settings")
        .factory("activityLogExportData", [
            "$resource",
            "ENV",
            activityLogExportData
        ]);
})(angular);

