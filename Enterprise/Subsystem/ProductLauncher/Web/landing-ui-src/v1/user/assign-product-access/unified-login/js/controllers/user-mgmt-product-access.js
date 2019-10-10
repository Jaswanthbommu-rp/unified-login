//  userMgmt ProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function UserMgmtProductAccess($scope, model, tabsMenu, tabsData, UMDataModel) {
        var vm = this;
        var panelName = "UserMgmt";

        vm.init = function() {
            vm.panelName = panelName;
            vm.model = model;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function() {
            return UMDataModel.isActive();
        };

        vm.setChanged = function() {
            UMDataModel.setChanged();
        };

        vm.destroy = function() {
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
        .controller("UserMgmtProductAccess", [
            "$scope",
            "userMgmtSideMenuConfig",
            "userMgmtTabsMenu",
            "userMgmtTabsData",
            "userMgmtDataModel",
            UserMgmtProductAccess
        ]);
})(angular);