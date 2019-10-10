//  Roles and Rights Controller

(function(angular, undefined) {
    "use strict";

    function RolesRightsCtrl($scope, tabsMenu, productsModel, productsData, security) {
        var vm = this;

        vm.init = function() {
            vm.loadTabs = false;
            vm.security = security;
            if (productsData.isReady()) {
                vm.setData();
                vm.productWatch = angular.noop;
            } else {
                vm.productWatch = productsData.subscribe(vm.setData);
            }

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.setData = function(data) {
            productsModel.setFamilies(productsData.getFamilies());
            productsModel.setSolutions(productsData.getSolutions());
            if (productsModel.isReady()) {
                vm.setProductModal();
                vm.productsModelWatch = angular.noop;
            } else {
                vm.productsModelWatch = productsModel.subscribe(vm.setProductModal);
            }

        };

        vm.setProductModal = function() {
            vm.loadTabs = true;
            vm.model = productsModel;
            vm.tabsMenu = tabsMenu;
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.productWatch();
            vm.productsModelWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("RolesRightsCtrl", [
            "$scope",
            "rolesRightsTabsMenu",
            "rolesAndRightsProductsModel",
            "productsDataModelExt",
            "routeSecurity",
            RolesRightsCtrl
        ]);
})(angular);
