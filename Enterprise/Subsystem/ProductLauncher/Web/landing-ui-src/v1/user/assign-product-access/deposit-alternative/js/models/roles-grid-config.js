//  Roles Grid Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function () {
            return [
            {
                key: "isAssigned",
                type: "custom",
                idKey: "id",
                templateUrl: "user/assign-product-access/deposit-alternative/templates/roles-radio.html"
            }, 
            {
                key: "name"
            }];
        };

        model.getHeaders = function () {
            return [
                [
                {
                    key: "isAssigned",
                    type: "select",
                    enabled: false
                }, 
                {
                    key: "name",
                    text: "Role",
                }             
                ]
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
                
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("daRolesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
