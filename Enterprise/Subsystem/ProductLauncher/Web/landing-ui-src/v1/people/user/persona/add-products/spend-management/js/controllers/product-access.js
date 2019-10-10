//  SMProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function SMProductAccessCtrl($scope, tabsMenuModel, navData, SMData) {
        var vm = this,
            panelName = "Spend Management";

        vm.init = function() {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.panelName = panelName;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.getActiveUrl = function() {
            return navData.getActiveUrl();
        };

        vm.destroy = function() {
            vm.destWatch();
            navData.reset();
            SMData.reset();
            panelName = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SMProductAccessCtrl", [
            "$scope",
            "rpScrollingTabsMenuModel",
            "SMNavModel",
            "SpendManagementDataModel",
            SMProductAccessCtrl
        ]);
})(angular);
