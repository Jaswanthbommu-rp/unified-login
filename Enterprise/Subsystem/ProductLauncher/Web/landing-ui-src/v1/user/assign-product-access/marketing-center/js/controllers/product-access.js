//  MCProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function MCProductAccess($scope, $filter, tabsMenuModel, navData, MCDataModel) {
        var vm = this;

        vm.init = function() {
            vm.panelName = $filter("productPanelText")("panelName.marketingCenter");
            vm.tabsList = [];
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.getActiveUrl = function() {
            return navData.getActiveUrl();
        };

        vm.isActive = function () {
            return MCDataModel.isActive();
        };

        vm.setChanged = function () {
            MCDataModel.setChanged();
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
        .controller("MCProductAccess", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "MCNavModel",
            "MarketingCenterDataModel",
            MCProductAccess
        ]);
})(angular);
