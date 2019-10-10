//  Investment Analytics ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function InvestmentAnalyticsProductAccessCtrl($scope, $filter, tabsMenu, tabsDatasvc, IADatamodel, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.productDisabled = false;
            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);
            vm.tabsMenu = tabsMenu().setData(tabsDatasvc.getList());
            vm.tabsList = tabsDatasvc.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.panelName = $filter("productPanelText")("panelName.investmentanalytics");
        };

        vm.isActive = function () {
            return IADatamodel.isActive();
        };

        vm.isReady = function () {
            return IADatamodel.isReady();
        };

        vm.setChanged = function () {
            IADatamodel.setChanged();
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.destroy = function () {
            //IADatamodel.reset();
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
        .controller("InvestmentAnalyticsProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "investmentAnalyticsTabsData",
            "investmentAnalyticsDataModel",
            "pubsub",
            InvestmentAnalyticsProductAccessCtrl
        ]);
})(angular);
