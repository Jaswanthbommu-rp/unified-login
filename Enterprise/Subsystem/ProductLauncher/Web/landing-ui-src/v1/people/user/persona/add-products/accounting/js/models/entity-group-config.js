//  Accounting Entity Group Grid Config Model

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
                key: "name",
                type: "text",
            }, {
                key: "infoTooltip",
                type: "custom",
                templateUrl: "people/user/persona/add-products/accounting/templates/entity-group-icon.html"
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "isAssigned",
                    type: "select",
                    enabled: true
                }, {
                    key: "name",
                    text: "Entity Group",
                }, {
                    key: "infoTooltip",
                    text: ""
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
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Entity Group Name"
                },
                { }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("AEntityGroupGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
