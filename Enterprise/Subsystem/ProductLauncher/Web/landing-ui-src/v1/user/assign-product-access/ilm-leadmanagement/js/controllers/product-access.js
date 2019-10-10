//  ILM Lead Management ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function ILMLeadManagementProductAccessCtrl($scope, $filter, tabsMenu, tabsDatasvc, ILMLMDatamodel, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenu().setData(tabsDatasvc.getList());
            vm.tabsList = tabsDatasvc.getList();
            vm.productDisabled = false;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.panelName = $filter("productPanelText")("panelName.ilmleadmanagement");
            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);
        };

        vm.isActive = function () {
            return ILMLMDatamodel.isActive();
        };

        vm.setChanged = function () {
            ILMLMDatamodel.setChanged();
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.destroy = function () {
            tabsDatasvc.reset();
            vm.destWatch();
            vm.productAccessWatch();
            vm.productDisabled = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ILMLeadManagementProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "ILMLeadManagementTabsData",
            "ilmLeadManagementDataModel",
            "pubsub",
            ILMLeadManagementProductAccessCtrl
        ]);
})(angular);
