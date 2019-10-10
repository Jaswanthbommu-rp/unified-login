//  assign roles Config Model

(function(angular) {
    "use strict";

    function factory(gridConfig, products) {
        var model = gridConfig();

        model.get = function() {
            return [{
                    key: "assigned",
                    type: "select",
                    idKey: "id"
                },                
                {
                    key: "description"
                }
            ];
        };

        model.getHeaders = function() {
            return [
                [{
                        key: "assigned",
                        type: "select",
                        enabled: true

                    },                    
                    {
                        key: "description",
                        text: "Right"
                    }
                ]
            ];
        };

        model.getFilters = function() {
            return [{
                    key: "assigned",
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
                    key: "description",
                    type: "text",
                    filterDelay: 0,
                    placeholder: "Search by right"
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
        .factory("userMgmtAssignRightsConfig", [
            "rpGridConfig",
            "userMgmtProductsData",
            factory
        ]);
})(angular);