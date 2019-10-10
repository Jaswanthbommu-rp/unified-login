//  Roles PropertyManagement Controller

(function(angular, undefined) {
    "use strict";

    function OnesiteRolesCtrl(
        $scope,
        $filter,
        model,
        onesiteRolesGridConfig,
        onesiteRolesGridActions,
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
            onesiteRolesGridConfig.setSrc(vm);
            onesiteRolesGridActions.setSrc(vm);
            vm.loadProductsData();
            vm.model = model;
            vm.model.initGrid();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

        };

        vm.hasAccess = function () {
            return security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess();
        };

        vm.createNewRole = function() {
            newRoleAside.show();
        };

        vm.cloneRole = function(role) {
            vm.model.cloneRole(role, cloneSvc);
        };

        vm.loadProductsData = function() {
            products.getProdData(productsSvc);
        };

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
        .controller("OnesiteRolesCtrl", [
            "$scope",
            "$filter",
            "onesiteRolesModel",
            "onesiteRolesGridConfig",
            "onesiteRolesGridActions",
            "onesiteNewRoleAside",
            "onesiteCloneRoleSvc",
            "onesiteDeleteRoleSvc",
            "productsData",
            "onesiteCentersProductsSvc",
            "routeSecurity",
            "personaDetails",
            OnesiteRolesCtrl
        ]);
})(angular);
