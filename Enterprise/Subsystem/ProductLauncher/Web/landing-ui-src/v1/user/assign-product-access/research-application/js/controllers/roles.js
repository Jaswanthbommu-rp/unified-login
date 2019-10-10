//  res-app Roles Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function ResAppRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, UMDataModel, userDetailsModel, tabsModel) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            tabsDataAll = ["roles"],
            tabsDataAnalyst = ["roles"],
            genericDataErrorReason = "";

        vm.init = function() {
            vm.showGoals("");
            vm.rolesGrid = rolesGrid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            rolesGridTransform.watch(rolesGrid);
            rolesGrid.setConfig(gridConfig);
            gridPagination.setGrid(rolesGrid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            vm.updateWatch = pubsub.subscribe("ra.roles-radio", vm.updateRecords);
        };

        vm.isActive = function() {
            return UMDataModel.isActive();
        };

        vm.loadData = function() {
            if (persona.isReady() && vm.isActive()) {
                rolesGrid.busy(true);
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

        vm.updateRecords = function(record) {

            vm.dataReq.records.forEach(function(item) {
                item.isAssigned = item.id == record.id;
            });
            UMDataModel.setRoles(vm.dataReq.records);
            vm.showGoals(record.name);
        };

        vm.showGoals = function(name) {

            // if (name.toLowerCase() === "executive") {
            //     tabsModel.setTabs(tabsDataAnalyst);
            // } else {
                tabsModel.setTabs(tabsDataAll);
            // }
        };

        vm.setData = function(resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                UMDataModel.setRoles(resp.records);
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

        vm.destroy = function() {
            vm.destWatch();
            vm.updateWatch();
            vm.personaWatch();
            rolesGrid.destroy();
            gridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            rolesGridTransform.destroy();

            vm = undefined;
            $scope = undefined;
            rolesGrid = undefined;
            gridPagination = undefined;
            rolesGridTransform = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ResAppRolesGridCtrl", [
            "$scope",
            "$filter",
            "resAppUserRolesSvc",
            "rpGridModel",
            "resAppRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "resAppDataModel",
            "userDetailsModel",
            "resAppTabsMenuModel",
            ResAppRolesGridCtrl
        ]);
})(angular);
