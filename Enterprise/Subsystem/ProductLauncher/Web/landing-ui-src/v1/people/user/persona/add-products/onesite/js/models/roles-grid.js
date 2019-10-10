//  Rights Grid Model

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
                key: "name"
            }, {
                key: "roletype"
            }, {
                key: "infoIcon",
                type: "custom",
                templateUrl: "people/user/persona/add-products/onesite/templates/rights-info-icon.html"
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
                    text: "Role",
                }, {
                    key: "roletype",
                    text: "Role Type",
                }, {
                    key: "infoIcon",
                    text: "",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "isAssigned",
                    value: "",
                    type: "menu",
                    size: "small",
                    options: [
                        {
                            value: "",
                            name: "All"
                        },
                        {
                            value: true,
                            name: "Assigned"
                        },
                        {
                            value: false,
                            name: "Unassigned"
                        }
                    ]
                },
                {
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Role Name"
                },
                {
                    key: "roletype",
                    type: "text",
                    placeholder: "Filter by Role Type"
                },
                {
                    key: "infoIcon",
                    type: "",
                    placeholder: ""
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("osRolesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
