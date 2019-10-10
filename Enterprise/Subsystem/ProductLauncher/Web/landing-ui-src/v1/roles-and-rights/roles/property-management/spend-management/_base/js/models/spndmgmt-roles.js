//  Roles Model

(function(angular, undefined) {
    "use strict";

    function factory(
        $filter,
        gridConfig,
        gridModel,
        gridTransformSvc,
        gridActions,
        gridPaginationModel,
        pubsub,
        svc,
        assignRolesAside,
        tabsContext,
        user,
        cloneRoleAside,
        cloneTabsContext,
        $modal,
        msgModel,
        msgModal,
        persona

    ) {
        var model = {},
            cfg = {
                recordsPerPage: 10
            };

        model.asideScope = {};

        model.init = function() {
            model.loadModal();
            model.spndmgmtNewRole = pubsub.subscribe("spndMgmtSettings.newRole", model.updateGrid);
            model.spndmgmtEditRole = pubsub.subscribe("spndMgmtSettings.editRole", model.updateGrid);
            model.spndmgmtEntRoles = pubsub.subscribe("spndMgmtSettings.entRoles", model.updateGrid);
            model.spndmgmtCloneRole = pubsub.subscribe("spndMgmtSettings.cloneRole", model.updateGrid);
            model.spndmgmtDeleteRole = pubsub.subscribe("spndMgmtSettings.deleteRole", model.updateGrid);


            return model;
        };

        model.initGrid = function() {
            var grid = gridModel(),
                gridTransform = gridTransformSvc(),
                gridPagination = gridPaginationModel();
            model.grid = grid;
            model.grid.busy(true);
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);

            gridPagination.setConfig(cfg);
            gridPagination.setGrid(grid).trackSelection(gridConfig.getTrackSelectionConfig());

            model.gridPagination = gridPagination;
            model.getDataSvc();
            return model;

        };

        model.updateGrid = function() {           
            model.grid.resetFilters();
            model.grid.busy(true);
            model.getDataSvc();
        };

        model.loadModal = function() {
            model.deleteRolesModal = $modal({
                show: false,
                placement: "center",
                templateUrl: "roles-and-rights/roles/property-management/spend-management/base/templates/delete-roles.html",
                backdrop: true,
                controller: "SpndMgmtDeleteModalCtrl",
                controllerAs: "ctrl"
            });

            model.deleteRoleModal = $modal({
                show: false,
                placement: "center",
                templateUrl: "roles-and-rights/roles/property-management/spend-management/base/templates/delete-role.html",
                backdrop: true,
                controller: "SpndMgmtDeleteModalCtrl",
                controllerAs: "ctrl"
            });

            model.warningRolesModal = $modal({
                show: false,
                placement: "center",
                templateUrl: "roles-and-rights/roles/property-management/spend-management/base/templates/delete-warning.html",
                backdrop: true,
                controller: "SpndMgmtDeleteModalCtrl",
                controllerAs: "ctrl"
            });

            model.defaultWarningModal = $modal({
                show: false,
                placement: "center",
                templateUrl: "roles-and-rights/roles/property-management/spend-management/base/templates/default-warning.html",
                backdrop: true,
                controller: "SpndMgmtDeleteModalCtrl",
                controllerAs: "ctrl"
            });

            model.notSelectedRolesModal = $modal({
                show: false,
                placement: "center",
                templateUrl: "roles-and-rights/roles/property-management/spend-management/base/templates/not-selected.html",
                backdrop: true,
                controller: "SpndMgmtDeleteModalCtrl",
                controllerAs: "ctrl"
            });
        };


        model.getDataSvc = function() {
            var params = { editorPersonaId: persona.getId() };
            svc.get(params, model.setDataFromSvc, model.setDataErr);
        };

        model.setDelRoles = function(data) {
            model.DelRoles = data.records;
        };

        model.getDelRoles = function() {
            return model.delRoles;
        };

        model.setDelRole = function(record) {
            model.delRecord = record;
        };

        model.getDelRole = function() {
            return model.delRecord;
        };

        model.showDeleteRolesConfirmModal = function() {
            model.deleteRolesModal.$promise.then(function() {
                model.deleteRolesModal.show();
            });
        };

        model.hideDeleteRolesConfirmModal = function() {
            model.deleteRolesModal.$promise.then(function() {
                model.deleteRolesModal.hide();
            });
        };

        model.showDeleteRoleConfirmModal = function() {
            model.deleteRoleModal.$promise.then(function() {
                model.deleteRoleModal.show();
            });
        };

        model.hideDeleteRoleConfirmModal = function() {
            model.deleteRoleModal.$promise.then(function() {
                model.deleteRoleModal.hide();
            });
        };


        model.showWarningModal = function() {
            model.warningRolesModal.$promise.then(function() {
                model.warningRolesModal.show();
            });
        };

        model.hideWarningModal = function() {
            model.warningRolesModal.$promise.then(function() {
                model.warningRolesModal.hide();
            });
        };

        model.showDefWarningModal = function() {
            model.defaultWarningModal.$promise.then(function() {
                model.defaultWarningModal.show();
            });
        };

        model.hideDefWarningModal = function() {
            model.defaultWarningModal.$promise.then(function() {
                model.defaultWarningModal.hide();
            });
        };



        model.showNotSelectedModal = function() {
            model.notSelectedRolesModal.$promise.then(function() {
                model.notSelectedRolesModal.show();
            });
        };

        model.hideNotSelectedModal = function() {
            model.notSelectedRolesModal.$promise.then(function() {
                model.notSelectedRolesModal.hide();
            });
        };

        model.checkRolesCustomOrDefault = function() {

            var selRoles = model.getSelectedRecords();
            var roles = model.getRoles();

            model.isDefaultRoleSelected = false;

            if (selRoles.selected.length > 0) {

                selRoles.selected.forEach(function(role) {
                    var selObj = $filter("filter")(roles, {
                        id: role
                    });

                    if (selObj.length > 0) {
                        selObj.forEach(function(roleObj) {                            
                                if ((roleObj.id === role) && (roleObj.roletype.toLowerCase() === "default")) {
                                model.isDefaultRoleSelected = true;
                                return model;
                            }
                        });
                    }
                });

            }

            return model;
        };

        model.checkRolesWarning = function() {

            var selRoles = model.getSelectedRecords();
            var roles = model.getRoles();

            model.isRolesAssignedToUsers = false;

            if (selRoles.selected.length > 0) {

                selRoles.selected.forEach(function(role) {

                    var selObj = $filter("filter")(roles, {
                        id: role
                    });

                    if (selObj.length > 0) {
                        selObj.forEach(function(roleObj) {
                            if ((roleObj.id === role) && (selObj[0].isAssigned === true)) {
                                model.isRolesAssignedToUsers = true;
                                return model;
                            }
                        });
                    }

                });

            }

            return model;
        };

        model.checkRoleWarning = function() {

            var selRole = model.getDelRole();
            var roles = model.getRoles();

            model.isRoleAssigned = false;

            if (selRole !== undefined) {

                var selObj = $filter("filter")(roles, {
                    id: selRole.id
                });

                if (selObj.length > 0) {
                    selObj.forEach(function(roleObj) {
                        if ((roleObj.id === selRole.id) && (selObj[0].isAssigned === true)) {
                            model.isRoleAssigned = true;
                            return model;
                        }
                    });
                }
            }

            return model;
        };

        model.setRolesNames = function() {
            model.rolesNames = "";

            var selRoles = model.getSelectedRecords();
            var roles = model.getRoles();

            selRoles.selected.forEach(function(role) {

                var selObj = $filter("filter")(roles, {
                    id: role
                });

                if (model.rolesNames === "") {
                    model.rolesNames = selObj[0].name;
                } else {
                    model.rolesNames += ", " + selObj[0].name;
                }
            });

            return model;
        };

        model.setRoleName = function() {
            model.roleName = "";
            var selRole = model.getDelRole();
            model.roleName = selRole.name;

            return model;
        };

        model.getSelectedRoles = function() {
            var selRoles = [];
            var selRoleIds = model.getSelectedRecords();

            var rolesList = model.getData();

            selRoleIds.selected.forEach(function(item) {
                var filter = {
                    roleId: item
                };

                var role = $filter("filter")(rolesList.records, filter);

                if (role.length > 0) {
                    selRoles.push(role[0]);
                }

            });
            return selRoles;
        };



        model.deleteSelectedRoles = function(dataSvcDel) {

            var selRoles = model.getSelectedRecords();

            if (selRoles.selected.length > 0) {
                selRoles.selected.forEach(function(roleId) {
                    var role = {
                        id: roleId
                    };
                    model.deleteRole(role, dataSvcDel);
                });

            }

        };


        model.getSelectedRecords = function() {
            return model.grid.getSelectionChanges();
        };

        model.editRole = function(record) {
            tabsContext.set({ type: "edit", data: record });
            assignRolesAside.show();
        };

        model.assignRightstoRole = function(record) {
            tabsContext.set({ type: "assign", data: record });
            assignRolesAside.show();
        };

        model.viewRightstoRole = function(record) {
            tabsContext.set({ type: "view", data: record });
            assignRolesAside.show();
        };

        model.cloneRole = function(role) {
            cloneTabsContext.set({ type: "clone", data: role });
            cloneRoleAside.show();
        };

        model.onDeleteRoleSuccess = function(resp) {               
            pubsub.publish("spndMgmtSettings.deleteRole");
        };

        model.onDeleteRoleError = function(resp) {
            
             var data = {
                msg: resp.data,
                title: "Error",
                success: false                
            };
             msgModel.setData(data);
             msgModal.show();
        };


        model.deleteRole = function(role, delSvc) {
            var data = {
                "editorPersonaId": persona.getId(),
                "roleId": role.id
            };
            delSvc.save(data, {}).$promise
                .then(model.onDeleteRoleSuccess, model.onDeleteRoleError);

        };
       
        model.setRoles = function(data) {
            model.data = data.records;
        };

        model.getRoles = function() {
            return model.data;
        };
        
        model.setDataFromSvc = function(data) {
            model.setRoles(data);
            model.gridPagination.flushBackupData();
            model.grid.busy(false);
            model.gridPagination.setData(data.records).goToPage({
                number: 0
            });
        };


        model.setDataErr = function(data) {
            logc("Error = > ", data);
        };

        model.resetGridFilters = function() {            
            model.grid.resetFilters();
        };

        model.resetGrid = function() {
            model.grid.destroy();
            model.gridPagination.destroy();            
        };

        model.reset = function() {
            model.grid.destroy();
            model.gridPagination.destroy();
            model.spndmgmtNewRole();
            model.spndmgmtEditRole();
            model.spndmgmtEntRoles();
            model.spndmgmtCloneRole();
            model.spndmgmtDeleteRole();
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("spndmgmtRolesModel", [
            "$filter",
            "spndmgmtRolesGridConfig",
            "rpGridModel",
            "rpGridTransform",
            "spndmgmtRolesGridActions",
            "rpGridPaginationModel",
            "pubsub",
            "spndmgmtRolesSvc",
            "spndmgmtAssignRoleAside",
            "spndmgmtAssignTabsContext",
            "userSessionModel",
            "spndmgmtCloneRoleAside",
            "spndmgmtCloneTabsContext",
            "$modal",
            "spndMgmtStatusMsgModel",
            "spndmgmtStatusMsgModal",
            "personaDetails",
            factory
        ]);
})(angular);