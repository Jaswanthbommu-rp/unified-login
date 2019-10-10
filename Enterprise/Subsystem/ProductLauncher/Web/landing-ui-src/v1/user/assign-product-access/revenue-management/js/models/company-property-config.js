//  Roles Config

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig, actions) {
        var model = gridConfig();

        model.get = function () {
            return [{
                    key: "companyName",
                    type: "text"
                },
                {
                    key: "assignedProperties",
                    type: "custom",
                    templateUrl: "user/assign-product-access/revenue-management/templates/properties-assigned.html"
                }
            ];
        };

        model.getHeaders = function () {
            return [
                [{
                        key: "companyName",
                        text: "Company",
                    },
                    {
                        key: "assignedProperties",
                        text: "Assigned Properties",
                    }
                ]
            ];
        };

        model.getFilters = function () {
            return [{
                    key: "companyName",
                    type: "text",
                    placeholder: "Filter by Company Name"
                },
                {
                    key: "assignedProperties"
                }
            ];
        };


        return model;
    }
    angular
        .module("settings")
        .factory("rmCompanyPropertyAssignedGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
