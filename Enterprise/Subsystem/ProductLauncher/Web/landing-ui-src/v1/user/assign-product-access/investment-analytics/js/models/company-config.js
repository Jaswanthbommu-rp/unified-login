//  Investment Analytics Markets Grid Config Model

(function (angular, undefined) {
    "use strict";

    function factory(gridConfig) {
        var model = gridConfig();

        model.get = function () {
            return [
                {
                    key: "companyName",
                    type: "text"
            }, {
                    key: "role",
                    type: "custom",
                    idKey: "companyId",
                    templateUrl: "user/assign-product-access/investment-analytics/templates/company-roles.html"
            }];
        };

        model.getHeaders = function () {
            return [
                [{
                    key: "companyName",
                    text: "Company",
                }, {
                    key: "role",
                    text: "Role"
                }]
            ];
        };

        model.getFilters = function () {
            return [

                {
                    key: "companyName",
                    type: "text",
                    placeholder: "Filter by Company Name"
                }
            ];
        };

        return model;
    }
    angular
        .module("settings")
        .factory("investmentAnalyticsCompanyGridConfig", [
            "rpGridConfig",
            factory
        ]);
})(angular);
