//  Leasing & Rents Settings Controller

(function (angular) {
    "use strict";

    function LrcSettingsCtrl($rootScope, $scope, $state, model) {
        var vm = this,
            watch1;

        vm.init = function () {
        	vm.navModel = model.navData;
            vm.$state = $state;

            vm.setSideMenuDefaultState();

            $scope.$on('$stateChangeSuccess', vm.setSideMenuDefaultState);

            watch1 = $scope.$on('$destroy', vm.destroy);
        };

        vm.setSideMenuDefaultState = function (event, toState, toParams, fromState, fromParams) {
            if ($state.is('product.lrc.settings')) {
                model.expandSideMenu();
            }
            vm.sideMenuCollapsed = model.getSideMenuState();
        };

        vm.destroy = function () {
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("LrcSettingsCtrl", [
            "$rootScope",
            "$scope",
            "$state",
        	"lrcSettingsModel",
        	LrcSettingsCtrl
        ]);
})(angular);