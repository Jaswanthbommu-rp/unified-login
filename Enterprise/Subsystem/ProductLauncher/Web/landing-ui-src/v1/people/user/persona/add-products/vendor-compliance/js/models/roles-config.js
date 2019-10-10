//  Roles Grid Config Model

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
                key: "description",
                type: "text",
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
                    key: "description",
                    text: "Description",
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
                    placeholder: "Filter by Role Name"
                },
                {
                    key: "description",
                    type: "text",
                    placeholder: "Filter by Description"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("VendCompRolesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
