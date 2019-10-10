//  new roles Config Model

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
                    key: "centerName"
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
                        key: "centerName",
                        text: "Product",
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
                    key: "centerName",
                    value: "",
                    type: "menu",
                    options: products.getProductsData()
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
        .factory("acctNewRightsConfig", [
            "rpGridConfig",
            "productsData",
            factory
        ]);
})(angular);