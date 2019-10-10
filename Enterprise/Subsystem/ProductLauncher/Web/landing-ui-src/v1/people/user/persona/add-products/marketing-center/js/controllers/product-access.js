//  MC Product Access Controller

(function(angular, undefined) {
    "use strict";

    function MCProductAccess($scope, tabsMenuModel, navData, MCDataModel) {
        var vm = this,
            panelName = "Marketing Center";

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
            MCDataModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("MCProductAccess", [
            "$scope",
            "rpScrollingTabsMenuModel",
            "MCNavModel",
            "MarketingCenterDataModel",
            MCProductAccess
        ]);
})(angular);
