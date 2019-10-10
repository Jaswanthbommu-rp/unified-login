//  Axometric ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function AXMProductAccessCtrl($scope, $filter, tabsMenu, tabsDatasvc, AXMDatamodel) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenu().setData(tabsDatasvc.getList());
            vm.tabsList = tabsDatasvc.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.panelName = $filter("productPanelText")("panelName.axiometrics");
        };

        vm.isActive = function () {
            return AXMDatamodel.isActive();
        };

        vm.isReady = function () {
            return AXMDatamodel.isReady();
        };

        vm.setChanged = function () {
            AXMDatamodel.setChanged();
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
        .controller("AXMProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "axmTabsData",
            "axmDataModel",
            AXMProductAccessCtrl
        ]);
})(angular);
