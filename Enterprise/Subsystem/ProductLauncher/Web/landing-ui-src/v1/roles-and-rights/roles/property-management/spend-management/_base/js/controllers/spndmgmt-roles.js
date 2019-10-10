//  Roles PropertyManagement Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtRolesCtrl(
        $scope,
        $filter,
        model,
        spndmgmtRolesGridConfig,
        spndmgmtRolesGridActions,
        newRoleAside,
        cloneSvc,
        delSvc,
        products,
        productsSvc,
        security,
        persona
    ) {

        var vm = this;

        vm.init = function() {
            spndmgmtRolesGridConfig.setSrc(vm);
            spndmgmtRolesGridActions.setSrc(vm);
            // vm.loadProductsData();
            vm.model = model;
            vm.model.initGrid();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

        };

        vm.hasAccess = function () {
            return security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess();
        };

        vm.hasViewAccess = function () {
            return security.isAllowed("viewRoleRight");
        };

        vm.createNewRole = function() {
            newRoleAside.show();
        };

        vm.cloneRole = function(role) {
            vm.model.cloneRole(role, cloneSvc);
        };

        // vm.loadProductsData = function() {
        //     products.getProdData(productsSvc);
        // };

        vm.deleteSelectedRoles = function() {

            var selRoles = model.getSelectedRecords();

            if (selRoles.selected.length > 0) {
                model.setDelRoles(selRoles);

                model.checkRolesCustomOrDefault();
                if (model.isDefaultRoleSelected) {
                    model.showDefWarningModal();
                    return;
                }

                model.checkRolesWarning();
                if (model.isRoleAssignedToRights) {
                    model.showWarningModal();
                    return;
                }

                model.setRolesNames();
                model.showDeleteRolesConfirmModal();

            } else {
                vm.model.action = "Delete";
                model.showNotSelectedModal();
            }

        };

        vm.deleteRole = function(role) {

            model.setDelRole(role);

            model.checkRoleWarning();

            if (model.isRoleAssigned) {
                model.showWarningModal();
                return;
            }
            model.setRoleName();
            model.showDeleteRoleConfirmModal();

        };

        vm.viewRights = function(record) {
            vm.model.viewRightstoRole(record);
        };

        vm.assignRights = function(record) {
            vm.model.assignRightstoRole(record);
        };

        vm.editRole = function(record) {
            vm.model.editRole(record);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm = undefined;
        };


        vm.init();
    }

    angular
        .module("settings")
        .controller("SpndMgmtRolesCtrl", [
            "$scope",
            "$filter",
            "spndmgmtRolesModel",
            "spndmgmtRolesGridConfig",
            "spndmgmtRolesGridActions",
            "spndmgmtNewRoleAside",
            "spndMgmtCloneRoleSvc",
            "spndMgmtDeleteRoleSvc",
            "productsData",
            "spndmgmtCentersProductsSvc",
            "routeSecurity",
            "personaDetails",
            SpndMgmtRolesCtrl
        ]);
})(angular);
