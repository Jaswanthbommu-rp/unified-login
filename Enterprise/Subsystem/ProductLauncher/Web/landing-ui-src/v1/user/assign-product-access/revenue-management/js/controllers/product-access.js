//  revenueManagement ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function RevenueManagementProductAccessCtrl($scope, $filter, tabsMenu, tabsDatasvc, RMDataModel, pubsub, templateModel) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.productDisabled = false;
            vm.productAccessWatch = pubsub.subscribe("ao.regUserNoEmailNotAllowed", vm.setProductDisabled);
            vm.tabsMenu = tabsMenu().setData(tabsDatasvc.getList());
            vm.tabsList = tabsDatasvc.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.panelName = $filter("productPanelText")("panelName.revenuemanagement");
        };

        vm.isActive = function () {
            return RMDataModel.isActive() && !templateModel.isProductExists(32);
        };

        vm.isReady = function () {
            return RMDataModel.isReady();
        };

        vm.setChanged = function () {
            RMDataModel.setChanged();
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.destroy = function () {
            tabsDatasvc.reset();
            vm.destWatch();
            vm.productAccessWatch();
            vm.productDisabled = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("RevenueManagementProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "revenueManagementTabsData",
            "revenueManagementDataModel",
            "pubsub",
            "productTemplateModel",
            RevenueManagementProductAccessCtrl
        ]);
})(angular);
