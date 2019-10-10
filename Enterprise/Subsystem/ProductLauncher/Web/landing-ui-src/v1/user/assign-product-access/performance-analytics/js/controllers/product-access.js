//  Performance Analytics ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function PerformanceAnalyticsProductAccessCtrl($scope, $filter, tabsMenu, tabsDatasvc, PADatamodel, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.productDisabled = false;
            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);
            vm.tabsMenu = tabsMenu().setData(tabsDatasvc.getList());
            vm.tabsList = tabsDatasvc.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.panelName = $filter("productPanelText")("panelName.performancenalytics");
        };

        vm.isActive = function () {
            return PADatamodel.isActive();
        };

        vm.isReady = function () {
            return PADatamodel.isReady();
        };

        vm.setChanged = function () {
            PADatamodel.setChanged();
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
        .controller("PerformanceAnalyticsProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "performanceAnalyticsTabsData",
            "performanceAnalyticsDataModel",
            "pubsub",
            PerformanceAnalyticsProductAccessCtrl
        ]);
})(angular);
