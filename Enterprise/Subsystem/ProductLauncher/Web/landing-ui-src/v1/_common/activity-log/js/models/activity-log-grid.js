//  Activity Log Grid Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig, security) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "icon",
                type: "custom",
                templateUrl: "common/activity-log/templates/activity-log-icon.html"
            }, {
                key: "message",
                type: "text"
            }, {
                key: "activityDate",
                type: "custom",
                templateUrl: "common/activity-log/templates/activity-log-timestamp.html"
            }, {
                key: "userDetails",
                type: "custom",
                templateUrl: "people/activity-log/templates/single-user-activity-link.html"
            }];
        };

        model.getHeaders = function() {
            return [
            ];
        };

        // model.getFilters = function () {
        //     return [
        //         {
        //             key: "isAssigned",
        //             value: "",
        //             type: "menu",
        //             size: "small",
        //             options: [
        //                 {
        //                     value: "",
        //                     name: "All"
        //                 },
        //                 {
        //                     value: true,
        //                     name: "Selected"
        //                 },
        //                 {
        //                     value: false,
        //                     name: "Not Selected"
        //                 }
        //             ]
        //         },
        //         {
        //             key: "name",
        //             type: "text",
        //             placeholder: "Filter by Role Name"
        //         },
        //         {
        //             key: "roletype",
        //             type: "text",
        //             placeholder: "Filter by Role Type"
        //         },
        //         {
        //             key: "infoIcon",
        //             type: "",
        //             placeholder: ""
        //         }
        //     ];
        // };

        return model;
    }
    angular
        .module("settings")
        .factory("activityLogGridConfig", [
            "rpGridConfig",
            "routeSecurity",
            factory
        ]);
})(angular);
