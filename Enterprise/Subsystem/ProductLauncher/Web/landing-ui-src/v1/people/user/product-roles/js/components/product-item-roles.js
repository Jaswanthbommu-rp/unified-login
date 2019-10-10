//  Product Item Roles Access Component

(function (angular) {
    "use strict";

    function ProductRolesAccessComponent(rolesGridConfig, rolesFormConfig, rolesMockData, rpGridModel, rpGridTransform) {
        var ctrl = this,
            defaultConfig = {
                grid: null,
                form: null
            },
            defaultModel = {
                searchText: ""
            };

        ctrl.$onInit = function() {
            logc("INIT: Product Roles Access");

            ctrl.product = angular.copy(ctrl.productItem);
            ctrl.model = angular.copy(defaultModel);
            
            ctrl.config = angular.copy(defaultConfig);
            ctrl.config.grid = ctrl.initGrid();
            ctrl.config.form = rolesFormConfig;
            ctrl.config.form.setMethodsSrc(ctrl);
        };

        ctrl.$onDestroy = function() {
            logc("DESTROY: Product Roles Access");
            ctrl.config.grid.destroy();
            ctrl.config.grid = undefined;

            ctrl.product = undefined;
            ctrl = undefined;
        };

        ctrl.initGrid = function() {
            var grid = rpGridModel(),
                gridTransform = rpGridTransform();

            gridTransform.watch(grid);
            grid.setConfig(rolesGridConfig);
            grid.setData({
                records: rolesMockData()
            });

            return grid;
        };

    }

    angular
        .module("settings")
        .component("rpProductItemRolesAccess", {
            templateUrl: "people/user/product-roles/templates/product-item-roles.html",
            bindings: {
                productItem: "<"
            },
            controller: [
                "productRolesGridConfig",
                "productRolesFormConfig",
                "productRolesListData",
                "rpGridModel",
                "rpGridTransform",
                ProductRolesAccessComponent
            ]
        });
        
})(angular);
