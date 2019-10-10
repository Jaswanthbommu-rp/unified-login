//  Accounting Entities Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "id"
            }, {
                key: "id",
                type: "text",
            }, {
                key: "name",
                type: "text",
            }, {
                key: "state",
                type: "text"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: true
                }, {
                    key: "id",
                    text: "ID",
                }, {
                    key: "name",
                    text: "Name",
                }, {
                    key: "state",
                    text: "State"
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    type: "menu",
                    value: "",
                    options: [
                        {
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
                    key: "id",
                    type: "text",
                    placeholder: "Filter by Entity ID"
                },
                {
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Entity Name"
                },
                {
                    key: "state",
                    type: "text",
                    placeholder: "Filter by State"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("AEntitiesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
