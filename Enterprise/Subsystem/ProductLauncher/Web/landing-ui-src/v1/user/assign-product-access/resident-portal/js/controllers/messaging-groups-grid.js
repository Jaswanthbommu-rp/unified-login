//  Messaging Groups Grid Tab Controller

(function(angular, undefined) {
    "use strict";

    function RPMessagingGroupsGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, ResPortDataModel, userDetailsModel, security) {
        var vm = this,
            messagingGroupsGrid = gridModel(),
            messagingGroupsGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function() {
            vm.messagingGroupsGrid = messagingGroupsGrid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            messagingGroupsGridTransform.watch(messagingGroupsGrid);
            messagingGroupsGrid.setConfig(gridConfig);
            gridPagination.setGrid(messagingGroupsGrid);
            $scope.gridPagination = gridPagination;
            gridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);

            if (persona.isReady()) {
                vm.hasResidentPortalUserAccess = persona.data.hasResidentPortalUserAccess;
                vm.loadData();
            } else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
        };

        vm.isActive = function() {
            return ResPortDataModel.isActive();
        };

        vm.loadData = function() {
            if (persona.isReady() && vm.isActive()) {
                messagingGroupsGrid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function(resp) {
            messagingGroupsGrid.busy(false);
            resp.data = vm.sortData(resp);
            if (resp.data && resp.data.length > 0) {
                if (security.isAllowed("viewUser") || !persona.data.hasResidentPortalUserAccess) {
                    resp.data.forEach(function(item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                gridPagination.setData(resp.data).goToPage({
                    number: 0
                });

                ResPortDataModel.setMessageGroups(resp.data);
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

        vm.sortData = function(resp) {
            return $filter("orderBy")(resp.data, "name");
        };

        vm.destroy = function() {
            vm.destWatch();
            messagingGroupsGrid.destroy();
            messagingGroupsGridTransform.destroy();
            gridPagination.destroy();
            messagingGroupsGrid = undefined;
            messagingGroupsGridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("RPMessagingGroupsGridCtrl", [
            "$scope",
            "$filter",
            "RPMessagingGroupsSvc",
            "rpGridModel",
            "RPMessagingGroupsGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "residentPortalsDataModel",
            "userDetailsModel",
            "routeSecurity",
            RPMessagingGroupsGridCtrl
        ]);
})(angular);