//  OnesiteProductAccess Controller

(function (angular, undefined) {
    "use strict";

    function OnesiteProductAccess($scope, $filter, model, tabsMenu, tabsData, OSDataModel) {
        var vm = this;

        vm.init = function () {
            vm.panelName = $filter("productPanelText")("panelName.onesite");
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return OSDataModel.isActive();
        };

        vm.setChanged = function () {
            OSDataModel.setChanged();
        };

        vm.destroy = function () {
            model.reset();
            tabsData.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("OnesiteProductAccess", [
            "$scope",
            "$filter",
            "sideMenuConfig",
            "osTabsMenu",
            "osTabsData",
            "onesiteDataModel",
            OnesiteProductAccess
        ]);
})(angular);
