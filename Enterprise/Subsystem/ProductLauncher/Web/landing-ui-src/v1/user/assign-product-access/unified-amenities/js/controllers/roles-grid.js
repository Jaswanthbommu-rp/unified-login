//  Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function UARolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, UADataModel, userDetailsModel, roleModel, security, tabsModel) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            tabsDataAll = ["properties", "roles"],
            tabsDataRolesOnly = ["roles"],
            allProperties = false,
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

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.updateWatch = pubsub.subscribe("ua.roles-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return UADataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                grid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    partyId: persona.data.organization.partyId
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
            vm.allProperties = record.hidePropertiesTab();
            UADataModel.setAllProperties(vm.allProperties);
            vm.setTabs(vm.allProperties);
        };

        vm.setData = function (resp) {
            vm.records = [];
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }

                vm.records = resp.records.map(function (role) {
                    return roleModel(role);
                });

                gridPagination.setData(vm.records).goToPage({
                    number: 0
                });

                vm.records.forEach(function(role) {
                    if (role.isRoleAssigned()) {
                        vm.setTabs(role.hidePropertiesTab());
                    }
                });
                UADataModel.setRoles(vm.records);
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
        };

        vm.setTabs = function (hidePropertiesTab) {
            if (hidePropertiesTab) {
                tabsModel.setTabs(tabsDataRolesOnly);
                vm.allProperties = true;
            }
            else {
                tabsModel.setTabs(tabsDataAll);                
                vm.allProperties = false;
            }
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageUnifiedAmenitiesProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
            vm.personaWatch();
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            grid = undefined;
            $scope = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("UARolesGridCtrl", [
            "$scope",
            "$filter",
            "UARolesSvc",
            "rpGridModel",
            "uaRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "unifiedAmenitiesProductAccessModel",
            "userDetailsModel",
            "uaRoleModel",
            "routeSecurity",
            "UnifiedAmenitiesTabsModel",
            UARolesGridCtrl
        ]);
})(angular);
