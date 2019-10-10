//  New Role Controller

(function (angular, undefined) {
    "use strict";

    function UserMgmtCloneRoleCtrl(
        $scope,
        model,
        pubsub,
        tabsData,
        tabsManager,
        $timeout,
        tabsMenu,
        roleConfig,
        products,
        security,
        persona
    ) {
        var vm = this;

        vm.init = function () {
            vm.tabsList = [];
            vm.model = model;
            vm.errorMsg = "";
            vm.isError = false;
            model.setRoleData();
            vm.roleConfig = roleConfig;
            vm.tabsMenu = tabsMenu;

            $timeout(vm.delayInit, 350);
            vm.formWatch = $scope.$watch("cloneRoleForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.errorWatch = pubsub.subscribe("settings.cloneRoleError", vm.onError);
        };

        vm.hasAccess = function () {
            return security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess();
        };

        vm.onError = function (resp) {
            vm.isError = true;
            vm.errorMsg = resp.errorReason;
        };

        vm.delayInit = function () {

            $timeout(tabsManager.init, 100);
            vm.tabsList = tabsData.getList();
        };

        vm.getActiveUrl = function () {
            return tabsData.getActiveUrl();
        };

        vm.cancel = function () {
            pubsub.publish("cloneRole.cancel");
        };

        vm.clone = function () {
            if (vm.form.$valid) {
                pubsub.publish("cloneRole.update");
            }
            else {
                vm.form.$setSubmitted();
            }

        };

        vm.setForm = function (form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.formWatch();
            vm.errorWatch();
            tabsManager.reset();
            model.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UserMgmtCloneRoleCtrl", [
            "$scope",
            "userMgmtCloneRoleModel",
            "pubsub",
            "userMgmtCloneTabsData",
            "userMgmtCloneRoleTabsManager",
            "$timeout",
            "userMgmtCloneRoleTabsMenu",
            "userMgmtCloneRoleFormConfig",
            "userMgmtProductsData",
            "routeSecurity",
            "personaDetails",
            UserMgmtCloneRoleCtrl
        ]);
})(angular);
