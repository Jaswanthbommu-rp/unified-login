//  assign roles Config Model

(function(angular) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                    key: "isAssigned",
                    type: "select",
                    idKey: "id"
                },
                {
                    key: "name"
                },
                {
                    key: "roletype"
                }
            ];
        };

        model.getHeaders = function() {
            return [
                [{
                        key: "isAssigned",
                        type: "select",
                        enabled: true
                    },
                    {
                        key: "name",
                        text: "Role",
                    },
                    {
                        key: "roletype",
                        text: "Type"
                    }
                ]
            ];
        };

        model.getFilters = function() {
            return [{
                    key: "isAssigned",
                    value: "",
                    type: "menu",
                    options: [{
                            value: "",
                            name: "All"
                        },
                        {
                            value: true,
                            name: "Selected"
                        },
                        {
                            value: false,
                            name: "Not Selected"
                        }
                    ]
                },
                {
                    key: "name",
                    type: "text",
                    filterDelay: 0,
                    placeholder: "Search by name"
                },
                {
                    key: "roletype",
                    value: "",
                    type: "menu",
                    options: [{
                            value: "",
                            name: "All"
                        },
                        {
                            value: "Custom",
                            name: "Custom"
                        },
                        {
                            value: "System",
                            name: "System"
                        }
                    ]
                }
            ];
        };

        model.getTrackSelectionConfig = function() {
            var config = {},
                columns = model.get();

            columns.forEach(function(column) {
                if (column.type == "select") {
                    config.idKey = column.idKey;
                    config.selectKey = column.key;
                }
            });

            return config;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("umAssignRolesToRightsConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);