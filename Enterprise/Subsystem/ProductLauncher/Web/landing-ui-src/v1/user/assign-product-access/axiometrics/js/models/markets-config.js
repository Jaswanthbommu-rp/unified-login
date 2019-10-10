//  Axiometrics Markets Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "isAssigned",
                type: "select",
                idKey: "groupId"
            }, {
                key: "groupName",
                type: "text"
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
                    key: "groupName",
                    text: "Market",
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
                    key: "groupName",
                    type: "text",
                    placeholder: "Filter by Market Name"
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
        .factory("axmMarketsGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
