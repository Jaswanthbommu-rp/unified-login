//  ILM LeadAnalytics ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function ILMLeadAnalyticsProductAccessCtrl($scope, $filter, tabsMenu, tabsDatasvc, ILMLADatamodel, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenu().setData(tabsDatasvc.getList());
            vm.tabsList = tabsDatasvc.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.productDisabled = false;
            vm.panelName = $filter("productPanelText")("panelName.ilmleadanalytics");
            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);
        };

        vm.isActive = function () {
            return ILMLADatamodel.isActive();
        };

        vm.isReady = function () {
            return ILMLADatamodel.isReady();
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.setChanged = function () {
            ILMLADatamodel.setChanged();
        };


        vm.destroy = function () {
            vm.productDisabled = undefined;
            tabsDatasvc.reset();
            vm.productAccessWatch();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ILMLeadAnalyticsProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "ILMLeadAnalyticsTabsData",
            "ilmLeadAnalyticsDataModel",
            "pubsub",
            ILMLeadAnalyticsProductAccessCtrl
        ]);
})(angular);
