//  Product Item Properties Access Component

(function (angular) {
    "use strict";

    function ProductPropertiesAccessComponent(propertiesGridConfig, propertiesFormConfig, propertiesMockData, rpGridModel, rpGridTransform) {
        var ctrl = this,
            defaultConfig = {
                grid: null,
                form: null
            },
            defaultModel = {
                searchText: ""
            };

        ctrl.$onInit = function() {
            logc("INIT: Product Properties Access");

            ctrl.product = angular.copy(ctrl.productItem);
            ctrl.model = angular.copy(defaultModel);
            
            ctrl.config = angular.copy(defaultConfig);
            ctrl.config.grid = ctrl.initGrid();
            ctrl.config.form = propertiesFormConfig;
            ctrl.config.form.setMethodsSrc(ctrl);
        };

        ctrl.$onDestroy = function() {
            logc("DESTROY: Product Properties Access");
            ctrl.config.grid.destroy();
            ctrl.config.grid = undefined;

            ctrl.product = undefined;
            ctrl = undefined;
        };

        ctrl.initGrid = function() {
            var grid = rpGridModel(),
                gridTransform = rpGridTransform();

            gridTransform.watch(grid);
            grid.setConfig(propertiesGridConfig);
            grid.setData({
                records: propertiesMockData()
            });

            return grid;
        };

    }

    angular
        .module("settings")
        .component("rpProductItemPropertiesAccess", {
            templateUrl: "people/user/product-properties/templates/product-item-properties.html",
            bindings: {
                productItem: "<"
            },
            controller: [
                "productPropertiesGridConfig",
                "productPropertiesFormConfig",
                "productPropertiesListData",
                "rpGridModel",
                "rpGridTransform",
                ProductPropertiesAccessComponent
            ]
        });
        
})(angular);
