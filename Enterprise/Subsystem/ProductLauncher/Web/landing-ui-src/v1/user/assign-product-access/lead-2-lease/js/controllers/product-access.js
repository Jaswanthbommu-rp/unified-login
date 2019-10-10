//  Lead2LeaseProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function Lead2LeaseProductAccess($scope, $filter, tabsMenuModel, navData, lead2LeaseDataModel) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.panelName = $filter("productPanelText")("panelName.lead2Lease");
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.getActiveUrl = function () {
            return navData.getActiveUrl();
        };

        vm.isActive = function () {
            return lead2LeaseDataModel.isActive();
        };

        vm.setChanged = function () {
            lead2LeaseDataModel.setChanged();
        };

        vm.destroy = function () {
            vm.destWatch();
            navData.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("Lead2LeaseProductAccess", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "lead2LeaseNavModel",
            "lead2LeaseDataModel",
            Lead2LeaseProductAccess
        ]);
})(angular);
