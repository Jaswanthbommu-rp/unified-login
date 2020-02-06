//  BusinessIntelligence ProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function BusinessIntelligenceProductAccessCtrl($scope, $filter, tabsMenu, tabsData, model, pubsub, userDetailsModel) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.productDisabled = false;
            vm.userDetailsModel = userDetailsModel;
            vm.productAccessWatch = pubsub.subscribe("ao.regUserNoEmailNotAllowed", vm.setProductDisabled);
            vm.tabsMenu = tabsMenu().setData(tabsData.getList());
            vm.tabsList = tabsData.getList();
            vm.panelName = $filter("productPanelText")("panelName.businessintelligence");
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return model.isActive();
        };

        vm.isReady = function () {
            return model.isReady();
        };

        vm.setChanged = function () {
            model.setChanged();
        };

        vm.setProductDisabled = function (value) {
            logc("bi value",value);
            vm.productDisabled = value;
        };

        vm.destroy = function () {
            tabsData.reset();
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
        .controller("BusinessIntelligenceProductAccessCtrl", [
            "$scope",
            "$filter",
            "rpScrollingTabsMenuModel",
            "BusinessIntelligenceTabsNavModel",
            "businessIntelligenceDataModel",
            "pubsub",
            "userDetailsModel",
            BusinessIntelligenceProductAccessCtrl
        ]);
})(angular);
