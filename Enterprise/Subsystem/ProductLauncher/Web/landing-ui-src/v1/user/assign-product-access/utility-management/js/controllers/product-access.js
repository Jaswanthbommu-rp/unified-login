//  UtilityManagementProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function UtilityManagementProductAccess($scope, tabsMenuModel, navData, dataModel) {
        var vm = this,
            panelName = "Utility Management";

        vm.init = function() {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.panelName = panelName;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return dataModel.isActive();
        };

        vm.setChanged = function () {
            dataModel.setChanged();
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
        .controller("UtilityManagementProductAccess", [
            "$scope",
            "rpScrollingTabsMenuModel",
            "UtilityManagementNavModel",
            "utilityManagementDataModel",
            UtilityManagementProductAccess
        ]);
})(angular);
