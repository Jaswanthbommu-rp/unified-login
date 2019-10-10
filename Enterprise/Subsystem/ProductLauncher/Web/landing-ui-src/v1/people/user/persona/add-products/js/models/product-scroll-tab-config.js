(function (angular) {
    "use strict";

    function factory($filter) {

        return function(id) {

            return [{
                id: "product-properties-" + id,
                type: "properties-tab",
                className: "",
                isActive: true,
                text: $filter("manageUserProductAccessText")("product_access_properties"),
                templateUrl: "people/user/persona/product-properties/index.html"
            }, {
                id: "product-roles-" + id,
                type: "roles-tab",
                className: "",
                isActive: false,
                text: $filter("manageUserProductAccessText")("product_access_roles"),
                templateUrl: "people/user/persona/product-roles/index.html"
            }];

        };
    }

    angular
        .module("settings")
        .factory("productScrollTabConfig", [
            "$filter",
        	factory
        ]);
})(angular);
