
(function (angular) {
    "use strict";

    function factory($filter) {
        var defaultPanel = {
            location: "",
            title: "",
            panelClassName: "",
        };

        var assignProducts = angular.copy(defaultPanel),
            productAccess = angular.copy(defaultPanel);

        assignProducts.location = "people/user/persona/add-products/templates/assign-prods-panel.html";
        assignProducts.title = $filter("manageUserPersonaText")("assign_products"); 
        assignProducts.panelClassName = "manage-user-product-assignment";

        productAccess.location = "people/user/persona/add-products/templates/product-access-panel.html";
        productAccess.title = $filter("manageUserPersonaText")("product_access"); 
        productAccess.panelClassName = "manage-user-product-access";

        return [
            assignProducts,
            productAccess
        ];
    }

    angular
        .module("settings")
        .factory("productPanelConfig", [
            "$filter",
            factory
        ]);
})(angular);
