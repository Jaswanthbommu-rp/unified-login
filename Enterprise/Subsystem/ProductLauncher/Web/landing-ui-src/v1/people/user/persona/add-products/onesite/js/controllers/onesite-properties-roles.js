//  Onesite Proerties and Roles Controller

(function(angular, undefined) {
    "use strict";

    function OSPropertiesRolesCtrl($scope, tabsMenu, tabsDataSvc) {
        var vm = this;

        vm.init = function() {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenu;
            vm.tabsList = tabsDataSvc.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.getActiveUrl = function() {
            return tabsDataSvc.getActiveUrl();
        };

        vm.destroy = function() {
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("OSPropertiesRolesCtrl", [
            "$scope",
            "osTabsMenu",
            "osTabsData",
            OSPropertiesRolesCtrl
        ]);
})(angular);
