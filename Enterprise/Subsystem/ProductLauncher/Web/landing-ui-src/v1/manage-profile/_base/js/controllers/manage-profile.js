//  Manage Profile Controller

(function (angular, undefined) {
    "use strict";

    function ManageProfileCtrl($scope, $timeout, $stateParams, pubsub, aside, tabsMenu, tabsDataSvc, tabsManager, user, userProfileModel) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.tabsMenu = tabsMenu;
            $timeout(vm.delayInit, 350);

            if ($stateParams.userId && userProfileModel.isProfileCardCaller()) {
                //means manage profile is accessed from edit user
                vm.username = userProfileModel.getUsername();
            }
            else {
                //mp is accessed from dashboard, load logged in user details
                vm.username = user.getUsername();
            }
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.delayInit = function () {
            tabsDataSvc.translate();
            $timeout(tabsManager.init, 100);
            vm.tabsList = tabsDataSvc.getList();
        };

        vm.getActiveUrl = function () {
            return tabsDataSvc.getActiveUrl();
        };

        vm.cancel = function () {
            pubsub.publish("mp.cancel");
        };

        vm.update = function () {
            pubsub.publish("mp.update");
        };

        vm.destroy = function () {
            vm.destWatch();
            tabsManager.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ManageProfileCtrl", [
            "$scope",
            "$timeout",
            "$stateParams",
            "pubsub",
            "mpAside",
            "mpTabsMenu",
            "mpTabsData",
            "mpTabsManager",
            "userSessionModel",
            "userProfileModel",
            ManageProfileCtrl
        ]);
})(angular);
