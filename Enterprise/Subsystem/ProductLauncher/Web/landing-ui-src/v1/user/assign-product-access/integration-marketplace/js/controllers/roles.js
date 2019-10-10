//  IntegMkt Roles Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function IntegMktRolesGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, UMDataModel, userDetailsModel, security) {
        var vm = this,
            rolesGrid = gridModel(),
            rolesGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel();

        vm.init = function () {

            vm.rolesGrid = rolesGrid;
            vm.rolesError = $filter("productPanelText")("panelError.generic");
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
            vm.updateWatch = pubsub.subscribe("intMkt.roles-radio", vm.updateRecords);
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

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
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
                vm.isRolesError = true;
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

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageIntegrationMarketplaceProductAccess;
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
        .controller("IntegMktRolesGridCtrl", [
            "$scope",
            "$filter",
            "integMktUserRolesSvc",
            "rpGridModel",
            "integMktRolesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "integMktDataModel",
            "userDetailsModel",
            "routeSecurity",
            IntegMktRolesGridCtrl
        ]);
})(angular);
