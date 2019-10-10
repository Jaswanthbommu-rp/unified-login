//  Product Item Component

(function (angular) {
    "use strict";

    function ProductItemAccessComponent(scrollTabConfig, rpTabsMenuModel) {
        var ctrl = this,
            defaultState = {
                isPanelActive: true,
                activeTab: "properties-tab"
            },
            defaultConfig = {
                scrollTabsMenu: null,
                scrollTabsConfig: null,
                form: null
            };

        ctrl.$onInit = function() {

            ctrl.product = angular.copy(ctrl.productItem);
            ctrl.state = angular.copy(defaultState);
            ctrl.config = angular.copy(defaultConfig);

            //init panel state
            // if(!angular.isUndefined(ctrl.displayPanel)) {
            //     ctrl.state.isPanelActive = ctrl.displayPanel;                
            // }

            //init scrolling tabs
            var scrollTabConfigInst = scrollTabConfig(ctrl.product.getId()); //TODO id should be dependent on selected account
            ctrl.config.scrollTabsMenu = rpTabsMenuModel();
            ctrl.config.scrollTabsMenu.setData(scrollTabConfigInst);
            ctrl.initTabChange();
        };

        ctrl.$onDestroy = function () {
            ctrl.config.scrollTabsMenu.destroy();
            ctrl.config.scrollTabsMenu = undefined;

            ctrl.product = undefined;
            ctrl = undefined;
        };

        ctrl.togglePanelState = function() {
            var isPanelActive = !ctrl.state.isPanelActive;
            if(isPanelActive) {
                ctrl.initTabChange();
            } else {
                ctrl.destroyTabChange();
            }

            ctrl.state.isPanelActive = isPanelActive;
        };

        ctrl.initTabChange = function() {
            ctrl.state.onScrollTabChange = ctrl.config.scrollTabsMenu.subscribe("change", ctrl.onTabChange);                
        };

        ctrl.destroyTabChange = function() {
            if(ctrl.state.onScrollTabChange && angular.isFunction(ctrl.state.onScrollTabChange)) {
                ctrl.state.onScrollTabChange();                
            }
        };

        ctrl.onTabChange = function(tab) {
            ctrl.state.activeTab = tab.type;

            //TODO SAVE ALL CHANGES TO THE MASTER LIST (personaProducts)
        };

    }

    angular
        .module("settings")
        .component("rpProductItemUserAccess", {
            templateUrl: "people/user/persona/add-products/templates/product-item-access.html",
            bindings: {
                productItem: "<"
            },
            controller: [
                "productScrollTabConfig",
                "rpScrollingTabsMenuModel",
                ProductItemAccessComponent
            ]
        });
        
})(angular);
