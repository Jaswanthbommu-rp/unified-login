//  OnSiteProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function OnSiteProductAccess($scope, $filter, tabsMenuModel, navData, dataModel) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.panelName = $filter("productPanelText")("panelName.on-site");
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.getActiveUrl = function () {
            return navData.getActiveUrl();
        };

        vm.isActive = function () {
            return dataModel.isActive();
        };

        vm.setChanged = function () {
            dataModel.setChanged();
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
        .controller("OnSiteProductAccess", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "onSiteNavModel",
            "onSiteDataModel",
            OnSiteProductAccess
        ]);
})(angular);
