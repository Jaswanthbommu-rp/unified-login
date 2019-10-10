//  VendCompProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function VendCompProductAccessCtrl($scope, $filter, tabsMenuModel, navData, VendCompDataModel) {
        var vm = this;

        vm.init = function() {
            vm.panelName = $filter("productPanelText")("panelName.vendorCompliance");
            vm.tabsList = [];
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return VendCompDataModel.isActive();
        };

        vm.setChanged = function () {
            VendCompDataModel.setChanged();
        };

        vm.getActiveUrl = function() {
            return navData.getActiveUrl();
        };

        vm.destroy = function() {
            vm.destWatch();
            navData.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("VendCompProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "VendCompNavModel",
            "vendorComplianceDataModel",
            VendCompProductAccessCtrl
        ]);
})(angular);
