//  Roles Grid config

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function () {
            return [            
            {
                key: "name"
            }, {
                key: "orgType"
            }, 
            {
                key: "orgsAssigned",
                type: "custom",
                templateUrl: "user/assign-product-access/click-pay/templates/roles-info-count.html"
            },
            {
                key: "orgTypeLink",
                type: "custom",
                templateUrl: "user/assign-product-access/click-pay/templates/roles-info-link.html"
            }];
        };

        model.getHeaders = function () {
            return [
                [                
                {
                    key: "name",
                    text: "Role",
                }, {
                    key: "orgType",
                    text: "Org Type",
                },
                {
                    key: "orgsAssigned",
                    text: "Assigned To",
                },
                {
                    key: "orgTypeLink",
                    text: "",
                }]
            ];
        };

        model.getFilters = function () {
            return [
                
                {
                    key: "name",
                    type: "text",
                    placeholder: "Filter by Role Name"
                },                
                 {
                    key: "orgType",
                    value: "",
                    type: "menu",
                    size: "small",
                    options: [
                        {
                            value: "",
                            name: "All"
                        },
                        {
                            value: "site",
                            name: "site"
                        },
                        {
                            value: "owner",
                            name: "owner"
                        },
                        {
                            value: "company",
                            name: "company"
                        }
                    ]
                },
                {
                    key: "orgsAssigned",
                    type: "",                    
                },
                {
                    key: "orgTypeLink",
                    type: "",                    
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("cpRolesGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
