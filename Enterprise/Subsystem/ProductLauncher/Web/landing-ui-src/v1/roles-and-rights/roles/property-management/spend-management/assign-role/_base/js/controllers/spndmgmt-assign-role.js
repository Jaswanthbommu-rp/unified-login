//  New Role Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtAssignRoleCtrl(
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
        vm.isError = false;

        vm.init = function() {

            vm.tabsList = [];
            vm.model = model;
            vm.model.setRoleData();
            vm.loadProductsData();
            vm.roleConfig = roleConfig;
            vm.tabsMenu = tabsMenu;
            vm.errorMsg = "";
            vm.isError = false;

            $timeout(vm.delayInit, 350);
            vm.formWatch = $scope.$watch("assignRoleForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.errorWatch = pubsub.subscribe("settings.osAssignRoleError", vm.onError);
        };

        vm.hasAccess = function () {
            return security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess();
        };

        vm.hasViewAccess = function () {
            return security.isAllowed("viewRoleRight");
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
            pubsub.publish("assignRole.cancel");
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };

        vm.update = function() {
            if (vm.form.$valid) {
                pubsub.publish("assignRole.update");
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
        .controller("SpndMgmtAssignRoleCtrl", [
            "$scope",
            "spndmgmtAssignRoleModel",
            "pubsub",
            "spndMgmtAssignTabsData",
            "spndMgmtAssignRoleTabsManager",
            "$timeout",
            "spndmgmtAssignRoleTabsMenu",
            "spndmgmtAssignRoleFormConfig",
            "productsData",
            "spndmgmtCentersProductsSvc",
            "routeSecurity",
            "personaDetails",
            SpndMgmtAssignRoleCtrl
        ]);
})(angular);
