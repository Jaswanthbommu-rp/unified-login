//  ProspectContactCenterProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function ProspectContactCenterProductAccess($scope, $filter, tabsMenuModel, navData, prospectContactCenterDataModel) {
        var vm = this;

        vm.init = function () {
            vm.panelName = $filter("productPanelText")("panelName.prospectContactCenter");
            vm.tabsList = [];
            vm.tabsMenu = tabsMenuModel().setData(navData.getList());
            vm.tabsList = navData.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.getActiveUrl = function () {
            return navData.getActiveUrl();
        };

        vm.isActive = function () {
            return prospectContactCenterDataModel.isActive();
        };

        vm.setChanged = function () {
            prospectContactCenterDataModel.setChanged();
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
        .controller("ProspectContactCenterProductAccess", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "prospectContactCenterNavModel",
            "prospectContactCenterDataModel",
            ProspectContactCenterProductAccess
        ]);
})(angular);
