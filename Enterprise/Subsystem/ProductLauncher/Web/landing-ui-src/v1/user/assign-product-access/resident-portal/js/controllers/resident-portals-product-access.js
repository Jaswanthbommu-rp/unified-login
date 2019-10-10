//  ResidentPortalsProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function ResidentPortalsProductAccess($scope, $filter, tabsModel, resPortDataModel) {
        var vm = this;

        vm.init = function () {
            vm.model = resPortDataModel;
            var tabs = ["properties", "roles", "messagingGroups", "notifications"];
            vm.panelName = $filter("productPanelText")("panelName.residentPortals");
            tabsModel.setTabs(tabs);
            tabsModel.activateTab("roles");
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return resPortDataModel.isActive();
        };

        vm.setChanged = function () {
            resPortDataModel.setChanged();
        };

        vm.destroy = function () {
            tabsModel.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ResidentPortalsProductAccess", [
            "$scope",
            "$filter",
            "ResPortTabsModel",
            "residentPortalsDataModel",
            ResidentPortalsProductAccess
        ]);
})(angular);
