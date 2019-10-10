//  AccountingProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function AccountingProductAccess($scope, tabsMenuModel, navData, ADataModel) {
        var vm = this,
            panelName = "Accounting";

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
            ADataModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AccountingProductAccess", [
            "$scope",
            "rpScrollingTabsMenuModel",
            "ANavModel",
            "AccountingDataModel",
            AccountingProductAccess
        ]);
})(angular);
