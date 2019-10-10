//  ResidentDocumentManagementccess Controller

(function (angular, undefined) {
    "use strict";

    function DocumentManagementProductAccess($scope, $filter,
        tabsModel, docMgmtDataModel, dmEvents) {
        var vm = this;

        vm.init = function () {
            vm.model = docMgmtDataModel;
            var initialTabs = ["roles"];
            vm.panelName = $filter("productPanelText")("panelName.documentManagement");
            tabsModel.setTabs(initialTabs);
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();

            vm.dmEvents = dmEvents.subscribe("tabsListChange", vm.updateTabs);

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.updateTabs = function (tabsList) {
            vm.tabsList = tabsModel.getTabsList();
            tabsModel.setTabs(tabsList);
        };

        vm.isActive = function () {
            return docMgmtDataModel.isActive();
        };

        vm.setChanged = function () {
            docMgmtDataModel.setChanged();
        };

        vm.destroy = function () {
            vm.tabsList = undefined;
            tabsModel.reset();
            vm.destWatch();
            vm.dmEvents();

            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("DocumentManagementProductAccess", [
            "$scope",
            "$filter",
            "DocumentManagementTabsModel",
            "documentManagementDataModel",
            "dmEvents",
            DocumentManagementProductAccess
        ]);
})(angular);
