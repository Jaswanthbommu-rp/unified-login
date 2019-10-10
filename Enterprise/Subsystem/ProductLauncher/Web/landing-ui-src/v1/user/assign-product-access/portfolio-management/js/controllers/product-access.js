//  PortfolioManagement ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function PortfolioManagementProductAccessCtrl($scope, $filter, tabsMenu, tabsDatasvc, PMDatamodel) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenu().setData(tabsDatasvc.getList());
            vm.tabsList = tabsDatasvc.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.panelName = $filter("productPanelText")("panelName.portfolioManagement");
        };

        vm.isActive = function () {
            return PMDatamodel.isActive();
        };

        vm.isReady = function () {
            return PMDatamodel.isReady();
        };

        vm.setChanged = function () {
            PMDatamodel.setChanged();
        };


        vm.destroy = function () {
            tabsDatasvc.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("PortfolioManagementProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "PortfolioManagementTabsData",
            "portfolioManagementDataModel",
            PortfolioManagementProductAccessCtrl
        ]);
})(angular);
