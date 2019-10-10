//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function DMRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, DocMgmtDataModel, userDetailsModel, tabsModel, roleModel, dmEvents, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            initialTabs = ["roles"],
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.tabNames = {
                "Department": "departments",
                "Site Name": "properties",
            };

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.gridSelectionWatch = vm.grid.subscribe("selectChange", vm.gridRowSelectionChange);
        };

        vm.gridRowSelectionChange = function (selectedRole) {
            var tabsList = angular.copy(initialTabs);
            vm.records.forEach(function (role) {
                if (selectedRole && 
                    role.roleType === selectedRole.roleType &&
                    role.id !== selectedRole.id)
                {
                    role.disableSelection = selectedRole.assigned;
                }
                if (role.assigned &&
                    role.roleType &&
                    vm.tabNames[role.roleType]) {
                    DocMgmtDataModel.setRoleID(role.roleType, role.id);
                    tabsList.push(vm.tabNames[role.roleType]);
                }
            });

            tabsModel.setTabs(tabsList);
            var updatedTabsList = tabsModel.getTabsList();
            dmEvents.publish("tabsListChange", tabsList);
        };

        vm.isActive = function () {
            return DocMgmtDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                grid.busy(true);
                var userPersonaId = userDetailsModel.getPersonaId();

                var params = {
                    userPersonaId: userPersonaId,
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.updateRecords = function (record) {
            vm.records.forEach(function (item) {
                item.setAssigned(item.hasId(record.getId()));
            });
        };

        vm.setData = function (resp) {
            vm.records = [];
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || !vm.userHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                vm.records = resp.records.map(function (role) {
                    return roleModel(role);
                });

                if (vm.userHasManageProductAccess()) {
                    vm.records.forEach(function (selectedRole) {
                        if (selectedRole.assigned) {
                            vm.records.forEach(function (role) {
                                if (role.roleType === selectedRole.roleType &&
                                    role.id !== selectedRole.id)
                                {
                                    role.disableSelection = selectedRole.assigned;
                                }
                            });
                        }
                    });
                }

                gridPagination.setData(vm.records).goToPage({
                    number: 0
                });

                DocMgmtDataModel.setRoles(vm.records);
            }
            if (resp.isError) {
                vm.isDataError = true;
                if (resp.errorReason !== "") {
                    vm.dataErrorReason = resp.errorReason;
                }
                else {
                    vm.dataErrorReason = genericDataErrorReason;
                }
            }

            DocMgmtDataModel.setTabsReady(true);
        };

        vm.userHasManageProductAccess = function () {
            return persona.data.hasManageDocumentManagementProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridSelectionWatch();
            grid.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            gridTransform.destroy();
            gridPagination.destroy();
            gridTransform = undefined;
            gridPagination = undefined;
            grid = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("DMRolesGridCtrl", [
            "$scope",
            "$filter",
            "DMRolesSvc",
            "rpGridModel",
            "docMgmtRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "documentManagementDataModel",
            "userDetailsModel",
            "DocumentManagementTabsModel",
            "dmRoleModel",
            "dmEvents",
            "routeSecurity",
            DMRolesGridCtrl
        ]);
})(angular);
