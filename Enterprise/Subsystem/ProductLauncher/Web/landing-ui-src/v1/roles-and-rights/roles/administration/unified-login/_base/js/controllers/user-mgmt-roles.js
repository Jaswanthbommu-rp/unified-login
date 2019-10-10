//  Roles PropertyManagement Controller

(function (angular, undefined) {
    "use strict";

    function UserMgmtRolesBaseCtrl(
        $scope,
        $filter,
        model,
        rolesGridConfig,
        rolesGridActions,
        newRoleAside,
        cloneSvc,
        delSvc,
        defSvc,
        security,
        persona
    ) {

        var vm = this;

        vm.init = function () {
            rolesGridConfig.setSrc(vm);
            rolesGridActions.setSrc(vm);

            vm.model = model;
            vm.model.initGrid();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

        };
        vm.createNewRole = function () {
            newRoleAside.show();
        };

        vm.cloneRole = function (role) {
            vm.model.cloneRole(role, cloneSvc);
        };


        vm.deleteSelectedRoles = function () {
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

            }
            else {
                vm.model.action = "Delete";
                model.showNotSelectedModal();
            }

        };

        vm.deleteRole = function (role) {
            model.setDelRole(role);
            model.checkRoleWarning();

            if (model.isRoleAssigned) {
                model.showWarningModal();
                return;
            }
            model.setRoleName();
            model.showDeleteRoleConfirmModal();

        };

        vm.setUserDefault = function(role) {
            vm.model.setUserDefault(role, defSvc);
        };

        vm.assignRights = function (record) {
            vm.model.assignRightstoRole(record);
        };

        vm.editRole = function (record) {
            vm.model.editRole(record);
        };

        vm.hasAccess = function () {
            return security.isAllowed("manageRoleRight") && !persona.hasViewOnlySupportToolAccess();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm = undefined;
        };


        vm.init();
    }

    angular
        .module("settings")
        .controller("UserMgmtRolesBaseCtrl", [
            "$scope",
            "$filter",
            "userMgmtRolesModel",
            "usMgmtRolesGridConfig",
            "userMgmtRolesGridActions",
            "userMgmtNewRoleAside",
            "userMgmtCloneRoleSvc",
            "userMgmtDeleteRoleSvc",
            "userMgmtSetDefaultRoleSvc",
            "routeSecurity",
            "personaDetails",
            UserMgmtRolesBaseCtrl
        ]);
})(angular);
