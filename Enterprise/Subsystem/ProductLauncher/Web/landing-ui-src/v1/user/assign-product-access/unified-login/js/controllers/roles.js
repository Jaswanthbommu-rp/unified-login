//  userMgmt Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function UserMgmtRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, UMDataModel, userDetailsModel, security) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
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
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
            vm.updateWatch = pubsub.subscribe("um.roles-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return UMDataModel.isActive();
        };

        vm.loadData = function () {
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

        vm.updateRecords = function (record) {
            vm.dataReq.records.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (resp) {
            rolesGrid.busy(false);
            if (resp.records && resp.records.length > 0) {

                if (security.isAllowed("viewUser")) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false
                        });
                        item.disabled = true;
                    });
                }else{
                    // vm.setEditorWithNoRightDisabled(resp);
                }

                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                UMDataModel.setRoles(resp.records);

                if (userDetailsModel.isClonedUser()) {
                    UMDataModel.setChanged();
                }
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

        vm.setEditorWithNoRightDisabled = function(data) {   
                data.records.forEach(function(item) {
                    angular.extend(item, { disabled: false });
                    if (item.isEditorHasRight === false) {                        
                        item.disabled = true;
                    }
                });
           
            return data;
        };

        vm.destroy = function () {
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
        .controller("UserMgmtRolesGridCtrl", [
            "$scope",
            "$filter",
            "userMgmtUserRolesSvc",
            "rpGridModel",
            "userMgmtRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "userMgmtDataModel",
            "userDetailsModel",
            "routeSecurity",
            UserMgmtRolesGridCtrl
        ]);
})(angular);
