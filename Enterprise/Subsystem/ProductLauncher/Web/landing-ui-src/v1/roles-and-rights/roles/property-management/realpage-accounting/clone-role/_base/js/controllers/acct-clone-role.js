//  Clone Role Controller

(function(angular, undefined) {
    "use strict";

    function AcctCloneRoleCtrl(
        $scope,
        model,
        pubsub,
        tabsData,
        tabsManager,
        $timeout,
        tabsMenu,
        roleConfig,
        products,
        productsSvc,
        security,
        persona
    ) {
        var vm = this;

        vm.init = function() {
            vm.tabsList = [];
            vm.model = model;
            vm.errorMsg = "";
            vm.isError = false;
            model.setRoleData();
            vm.loadProductsData();
            vm.roleConfig = roleConfig;
            vm.tabsMenu = tabsMenu;
            $timeout(vm.delayInit, 350);
            vm.formWatch = $scope.$watch("cloneRoleForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.errorWatch = pubsub.subscribe("osaSettings.cloneRoleError", vm.onError);
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
            vm.tabsList = tabsData.getList();
        };

        vm.getActiveUrl = function() {
            return tabsData.getActiveUrl();
        };

        vm.cancel = function() {
            pubsub.publish("cloneRole.cancel");
        };

        vm.clone = function() {
            if (vm.form.$valid) {
                pubsub.publish("cloneRole.update");
            } else {
                vm.form.$setSubmitted();
            }
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
                //$scope.rpTrackFormChanges.setData(vm.model);
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
        .controller("AcctCloneRoleCtrl", [
            "$scope",
            "acctCloneRoleModel",
            "pubsub",
            "acctCloneTabsData",
            "acctCloneRoleTabsManager",
            "$timeout",
            "acctCloneRoleTabsMenu",
            "acctCloneRoleFormConfig",
            "productsData",
            "acctCentersProductsSvc",
            "routeSecurity",
            "personaDetails",
            AcctCloneRoleCtrl
        ]);
})(angular);
