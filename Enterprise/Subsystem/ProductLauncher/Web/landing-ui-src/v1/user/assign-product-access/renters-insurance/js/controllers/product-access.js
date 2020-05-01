//  RIProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function RentersInsuranceProductAccessCtrl($scope, $filter, rentInsDataModel, tabsModel, templateModel) {
        var vm = this;

        vm.init = function () {
            var tabs = ["properties", "roles"];
            vm.panelName = $filter("productPanelText")("panelName.rentersInsurance");
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            tabsModel.setTabs(tabs);
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();
        };

        vm.isActive = function () {
            return rentInsDataModel.isActive() && !templateModel.isProductExists(15);
        };

        vm.setChanged = function () {
            rentInsDataModel.setChanged();
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
        .controller("RentersInsuranceProductAccessCtrl", [
            "$scope",
            "$filter",
            "rentersInsuranceDataModel",
            "RentersInsuranceTabsModel",
            "productTemplateModel",
            RentersInsuranceProductAccessCtrl
        ]);
})(angular);
