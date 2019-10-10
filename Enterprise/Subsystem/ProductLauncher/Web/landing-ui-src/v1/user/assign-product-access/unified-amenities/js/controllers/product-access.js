//  Unified Amenities Controller

(function (angular, undefined) {
    "use strict";

    function UnifiedAmenitiesProductAccessCtrl($scope, $filter, model, tabsModel) {
        var vm = this;

        vm.init = function () {
            vm.panelName = $filter("productPanelText")("panelName.unifiedAmenities");
            var tabs = ["properties", "roles"];
            tabsModel.setTabs(tabs);
            tabsModel.activateTab("roles");
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return model.isActive();
        };

        vm.setChanged = function () {
            model.setChanged();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UnifiedAmenitiesProductAccessCtrl", [
            "$scope",
            "$filter",
            "unifiedAmenitiesProductAccessModel",
            "UnifiedAmenitiesTabsModel",
            UnifiedAmenitiesProductAccessCtrl]);
})(angular);
