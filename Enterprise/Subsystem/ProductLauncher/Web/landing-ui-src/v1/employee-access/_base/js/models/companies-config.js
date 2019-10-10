//  Companies Config

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig, actions) {
        var model = gridConfig();

        model.get = function() {
            return [{
                    key: "companyName"
                },
                {
                    key: "address"                    
                },
                {
                    key: "city"
                },
                {
                    key: "state"
                },
                {
                    key: "postalCode"
                },
                {
                    key: "phoneNumber"
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
                [{
                        key: "companyName",
                        text: "Name",
                        isSortable: true
                    },
                    {
                        key: "address",
                        text: "Address",
                    },
                    {
                        key: "city",
                        text: "City",
                        isSortable: true
                    },
                    {
                        key: "state",
                        text: "State",
                        isSortable: true
                    },
                    {
                        key: "postalCode",
                        text: "Postal",
                        isSortable: true
                    },
                    {
                        key: "phoneNumber",
                        text: "Phone"
                    },
                    {
                        key: "more",
                        text: "Action"
                    }
                ]
            ];
        };

        model.getFilters = function() {
            return [{
                    key: "companyName"                    
                },
                {
                    key: "address",
                    
                },
                {
                    key: "city"
                },
                {
                    key: "state"
                },
                {
                    key: "postalCode"
                },
                {
                    key: "phoneNumber"
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
        .factory("empAccessCompGridConfig", [
            "rpGridConfig",
            "empAccessGridActions",
            factory
        ]);
})(angular);