//  VendCompProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function VendCompProductAccessCtrl($scope, tabsMenuModel, navData, VendCompDataModel) {
        var vm = this,
            panelName = "Vendor Credentialing";

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
            VendCompDataModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("VendCompProductAccessCtrl", [
            "$scope",
            "rpScrollingTabsMenuModel",
            "VendCompNavModel",
            "VendorComplianceDataModel",
            VendCompProductAccessCtrl
        ]);
})(angular);
