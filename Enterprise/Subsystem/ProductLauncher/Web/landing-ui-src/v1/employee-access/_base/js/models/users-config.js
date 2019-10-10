//  Companies Config

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig, actions) {
        var model = gridConfig();

        model.get = function() {
            return [
                {
                    key: "name"                    
                },
                {
                    key: "companyName"
                },
                {
                    key: "userType"
                },
                {
                    key: "userName"
                },
                {
                    key: "emailId"
                },
                {
                    key: "name3rdPartyIDP"
                },
                {
                    key: "more",
                    type: "actionsMenu",
                    getActions: actions.get,
                    toggleClassNames : "rp-icon-ellipsis-v"
                }
            ];
        };

        model.getHeaders = function() {
            return [
                [
                    {
                        key: "name",
                        text: "Name",
                        isSortable: true
                    },
                    {
                        key: "companyName",
                        text: "Company",
                        isSortable: true
                    },
                    {
                        key: "userType",
                        text: "User Type",
                        isSortable: true
                    },
                    {
                        key: "userName",
                        text: "UserName",
                        isSortable: true
                    },
                    {
                        key: "emailId",
                        text: "Notification Email",
                        isSortable: true
                    },
                    {
                        key: "name3rdPartyIDP",
                        text: "Third Party IDP",
                        isSortable: true
                    },
                    {
                        key: "more",
                        text: "Action"
                    }
                ]
            ];
        };

        model.getFilters = function() {
            return [
                {
                    key: "name",
                    
                },
                {
                    key: "companyName"                    
                },
                {
                    key: "userType"
                },
                {
                    key: "userName"
                },
                {
                    key: "emailId"
                },
                {
                    key: "name3rdPartyIDP"
                },
                {
                    key: "more"
                }
            ];
        };

        model.getTrackSelectionConfig = function() {
            var config = {},
                columns = model.get();

            columns.forEach(function(column) {
                if (column.type == "select") {
                    config.idKey = column.id;
                    config.selectKey = column.key;
                }
            });

            return config;
        };

        return model;
    }
    angular
        .module("settings")
        .factory("empAccessUserGridConfig", [
            "rpGridConfig",
            "empAccessUsersGridActions",
            factory
        ]);
})(angular);