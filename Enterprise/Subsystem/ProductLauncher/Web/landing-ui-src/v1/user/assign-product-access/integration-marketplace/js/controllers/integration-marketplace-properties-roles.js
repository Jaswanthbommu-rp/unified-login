//  IntegMktpls Proerties and Roles Controller

(function(angular, undefined) {
    "use strict";

    function IntegMktplsRolesCtrl($scope, tabsMenu, tabsDataSvc) {
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
        .controller("IntegMktRolesCtrl", [
            "$scope",
            "integMktTabsMenu",
            "integMktTabsData",
            IntegMktplsRolesCtrl
        ]);
})(angular);