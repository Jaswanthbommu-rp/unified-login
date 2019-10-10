//  Deposit Alt Controller

(function (angular, undefined) {
    "use strict";

    function DepositAltProductAccessCtrl($scope, $filter, model, tabsModel, pubsub) {
        var vm = this;

        vm.init = function () {
            vm.panelName = $filter("productPanelText")("panelName.depositAlternative");
            var tabs = ["roles", "properties", "areas", "regions", "notifications"];
            tabsModel.setTabs(tabs);
            tabsModel.activateTab("roles");
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();

            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return model.isActive();
        };

        vm.setChanged = function () {
            model.setChanged();
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.destroy = function () {
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
        .controller("DepositAltProductAccessCtrl", [
            "$scope",
            "$filter",
            "depositAlternativeProductAccessModel",
            "DepositAlternativeTabsModel",
            "pubsub",
            DepositAltProductAccessCtrl]);
})(angular);
