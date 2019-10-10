//  New Role Controller

(function(angular, undefined) {
    "use strict";

    function AcctNewRoleCtrl(
        $scope,
        model,
        pubsub,
        tabsConfig,
        tabsManager,
        $timeout,
        tabsMenu,
        newRoleTemp,
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

            model.newRole = new newRoleTemp.newRole();
            vm.roleConfig = roleConfig;
            vm.tabsMenu = tabsMenu;

            $timeout(vm.delayInit, 350);
            vm.formWatch = $scope.$watch("newRoleForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.errorWatch = pubsub.subscribe("osaSettings.newRoleError", vm.onError);
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
            pubsub.publish("newRoleAcct.cancel");
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
                pubsub.publish("newRoleAcct.update");
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
        .controller("AcctNewRoleCtrl", [
            "$scope",
            "acctNewRoleModel",
            "pubsub",
            "AcctNewRoleTabsData",
            "acctNewRoleTabsManager",
            "$timeout",
            "acctNewRoleTabsMenu",
            "acctNewRoleTempModel",
            "acctNewRoleFormConfig",
            "productsData",
            "routeSecurity",
            "personaDetails",
            AcctNewRoleCtrl
        ]);
})(angular);
