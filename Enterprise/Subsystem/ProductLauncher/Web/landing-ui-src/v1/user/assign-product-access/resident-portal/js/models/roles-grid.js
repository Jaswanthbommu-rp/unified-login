//  Roles Grid Config Model

(function(angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function() {
            return [{
                key: "assigned",
                type: "custom",
                idKey: "id",
                templateUrl: "user/assign-product-access/resident-portal/templates/role-radio.html"
            }, {
                key: "name",
                type: "text",
            }];
        };

        model.getHeaders = function() {
            return [
                [{
                    key: "assigned",
                    type: "select",
                    enabled: false
                }, {
                    key: "name",
                    text: "Role",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: "assigned",
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
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("resportRolesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
