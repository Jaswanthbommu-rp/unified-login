//  New Role Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtNewRoleCtrl(
        $scope,
        model,
        pubsub,
        tabsConfig,
        tabsManager,
        $timeout,
        tabsMenu,
        roleConfig,
        products,
        security,
        persona
    ) {
        var vm = this;

        vm.init = function() {
            vm.tabsList = [];
            vm.model = model;
            vm.errorMsg = "";
            vm.isError = false;

            vm.roleConfig = roleConfig;
            vm.tabsMenu = tabsMenu;

            $timeout(vm.delayInit, 350);
            vm.formWatch = $scope.$watch("newRoleForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.errorWatch = pubsub.subscribe("smSettings.newRoleError", vm.onError);
        };

        vm.hasAccess = function () {
            return security.isAllowed("manageRoleRight")  && !persona.hasViewOnlySupportToolAccess();
        };

        vm.onError = function(resp) {
            vm.isError = true;
            vm.errorMsg = resp.errorReason;
        };

        vm.loadProductsData = function() {
            products.getProductsData();
        };

        vm.delayInit = function() {

            $timeout(tabsManager.init, 100);
            vm.tabsList = tabsConfig.getList();
        };

        vm.getActiveUrl = function() {
            return tabsConfig.getActiveUrl();
        };

        vm.cancel = function() {
            pubsub.publish("newRoleSpndMgmt.cancel");
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
                //$scope.rpTrackFormChanges.setData(vm.model);
            }
        };

        vm.create = function() {

            if (vm.form.$valid) {
                pubsub.publish("newRoleSpndMgmt.update");
            } else {
                vm.form.$setSubmitted();
            }

        };

        vm.destroy = function() {
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
        .controller("SpndMgmtNewRoleCtrl", [
            "$scope",
            "spndmgmtNewRoleModel",
            "pubsub",
            "spndMgmtNewRoleTabsData",
            "spndMgmtNewRoleTabsManager",
            "$timeout",
            "spndmgmtNewRoleTabsMenu",
            "spndmgmtNewRoleFormConfig",
            "productsData",
            "routeSecurity",
            "personaDetails",
            SpndMgmtNewRoleCtrl
        ]);
})(angular);
