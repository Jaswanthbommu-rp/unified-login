//  Performance Analytics Markets Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.keys = {
            companyName: "companyName",
            role: "role",
            bmrole: "bmrole"
        };

        model.get = function () {
            return [
                {
                    key: model.keys.companyName,
                    type: "text"
            }, {
                    key: model.keys.role,
                    type: "custom",
                    idKey: "companyId",
                    templateUrl: "user/assign-product-access/performance-analytics/templates/company-roles.html"
            }, {
                    key: model.keys.bmrole,
                    type: "custom",
                    idKey: "companyId",
                    templateUrl: "user/assign-product-access/performance-analytics/templates/company-bm-roles.html"
            }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: model.keys.companyName,
                    text: "Company",
                }, {
                    key: model.keys.role,
                    text: "Role"
                }, {
                    key: model.keys.bmrole,
                    text: ""
                }]
            ];
        };

        model.getFilters = function () {
            return [
                {
                    key: model.keys.companyName,
                    type: "text",
                    placeholder: "Filter by Company Name"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("performanceAnalyticsCompanyGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
