//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function RPRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, ResPortDataModel, userDetailsModel, tabsModel, roleModel, security, switchConfig) {
        var vm = this,
            isAssignedCurrentNewPropAutomatically=false,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            tabsDataAll = ["properties", "roles", "messagingGroups", "notifications"],
            tabsDataEnterprise = ["properties", "roles"],
            tabsDataEnterpriseAdmin = ["roles"],
            tabsDataInit = ["roles"],
            genericDataErrorReason = "";

        vm.init = function () {
            vm.grid = grid;
            vm.isSelectedEnterpriseStdRole = false;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            gridTransform.watch(grid);
            grid.setConfig(gridConfig);
            gridPagination.setGrid(grid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.propertiesReadyWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.newPropSwitchWatch = pubsub.subscribe("roles.setPropSwitch", vm.setPropSwitch);

            if (persona.isReady()) {
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.updateWatch = pubsub.subscribe("rp.roles-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return ResPortDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                grid.busy(true);
                vm.assignPropSwitch = switchConfig({
                    onChange: vm.setNewPropAutoToggle,
                });
                var userPersonaId = userDetailsModel.getPersonaId();

                if (persona.hasResidentPortalUserAccess() && !userDetailsModel.userExists()) {
                    userPersonaId = persona.getId();
                }

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
            vm.isSelectedEnterpriseStdRole = false;
            vm.records.forEach(function (item) {
                item.setAssigned(item.hasId(record.getId()));
            });

            if (record.isEnterpriseAdmin()) {
                vm.isAssignedCurrentNewPropAutomatically = false;
                tabsModel.setTabs(tabsDataEnterpriseAdmin);
            } else if (record.isEnterprise()) {
                vm.isSelectedEnterpriseStdRole = true;
                vm.isAssignedCurrentNewPropAutomatically = false;
                tabsModel.setTabs(tabsDataEnterprise);
            } else {
                tabsModel.setTabs(tabsDataAll);
            }

            if (record.isEnterpriseAdmin()) {
                pubsub.publish("resPort.allProperties", true);
                ResPortDataModel.currentAdmin = true;
            } else if ((!record.isEnterpriseAdmin() && ResPortDataModel.currentAdmin) || (!record.isEnterprise() && vm.isAssignedCurrentNewPropAutomatically)) {
                vm.isAssignedCurrentNewPropAutomatically = false;
                ResPortDataModel.currentAdmin = false;
                pubsub.publish("resPort.allProperties", false);
            }
        };

        vm.setData = function (resp) {
            var found = false;
            if (ResPortDataModel.propertiesReady) {
                grid.busy(false);
            } else {
                vm.propertiesReadyWatch = pubsub.subscribe("rp.properties-ready", vm.propertiesReady);
            }
            vm.records = [];
            if (resp.data && resp.data.length > 0) {
                if (security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess) {
                    resp.data.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }

                vm.records = resp.data.map(function (role) {
                    return roleModel(role);
                });

                gridPagination.setData(vm.records).goToPage({
                    number: 0
                });

                vm.records.forEach(function (role) {
                    if (role.isAssigned() && role.isEnterpriseAdmin()) {
                        tabsModel.setTabs(tabsDataEnterpriseAdmin);
                        ResPortDataModel.currentAdmin = true;
                        ResPortDataModel.currentEnterpriseStd = false;
                        found = true;
                    } else if (role.isAssigned() && role.isEnterprise()) {
                        vm.isSelectedEnterpriseStdRole = true;
                        tabsModel.setTabs(tabsDataEnterprise);
                        ResPortDataModel.currentAdmin = false;
                        ResPortDataModel.currentEnterpriseStd = true;
                        found = true;
                    } else if (role.isAssigned()) {
                        tabsModel.setTabs(tabsDataAll);
                        ResPortDataModel.currentAdmin = false;
                        ResPortDataModel.currentEnterpriseStd = false;
                        found = true;
                    }
                });
                ResPortDataModel.setRoles(vm.records);
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
            if (!found) {
                tabsModel.setTabs(tabsDataInit);
            }
            ResPortDataModel.setTabsReady(true);
        };

        vm.propertiesReady = function (val) {
            if (val) {
                vm.propertiesReadyWatch();
                grid.busy(false);
            }
        };
        vm.setPropSwitch = function (val) {
            vm.isAssignedCurrentNewPropAutomatically = val;
            if (val) {
                tabsModel.setTabs(tabsDataEnterpriseAdmin);
            } else {
                tabsModel.setTabs(tabsDataEnterprise);
                pubsub.publish("resPortGrid.hidePropertiesGrid", false);
            }
        };
        vm.setNewPropAutoToggle = function (val) {
            if (val) {
                tabsModel.setTabs(tabsDataEnterpriseAdmin);
                pubsub.publish("resPort.allProperties", true);
            }
            else {
                tabsModel.setTabs(tabsDataEnterprise);
                pubsub.publish("resPort.allProperties", false);
            }
        };
        vm.destroy = function () {
            vm.destWatch();
            vm.propertiesReadyWatch();
            grid.destroy();
            vm.updateWatch();
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
        .controller("RPRolesGridCtrl", [
            "$scope",
            "$filter",
            "RPRolesSvc",
            "rpGridModel",
            "resportRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "residentPortalsDataModel",
            "userDetailsModel",
            "ResPortTabsModel",
            "roleModel",
            "routeSecurity",
            "rpSwitchConfig",
            RPRolesGridCtrl
        ]);
})(angular);