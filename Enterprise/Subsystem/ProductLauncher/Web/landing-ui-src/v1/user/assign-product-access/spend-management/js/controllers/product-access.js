//  SMProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function SMProductAccessCtrl($scope, $filter, tabsMenuModel, navData, SMData) {
        var vm = this;

        vm.init = function() {
            vm.panelName = $filter("productPanelText")("panelName.spendManagement");
            vm.tabsList = [];
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return SMData.isActive();
        };

        vm.setChanged = function () {
            SMData.setChanged();
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
        .controller("SMProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "SMNavModel",
            "spendManagementDataModel",
            SMProductAccessCtrl
        ]);
})(angular);
