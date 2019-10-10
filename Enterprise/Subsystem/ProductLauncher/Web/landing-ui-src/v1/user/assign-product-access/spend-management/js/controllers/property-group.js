//  Property Group Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function SMPropertyGroupGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, pubsub, persona, SMDataModel, userDetailsModel, security) {
        var vm = this,
            propertyGroupGrid = gridModel(),
            propertyGroupGridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.propertyGroupGrid = propertyGroupGrid;
            genericDataErrorReason = $filter("productPanelText")("panelError.generic");
            propertyGroupGridTransform.watch(propertyGroupGrid);
            propertyGroupGrid.setConfig(gridConfig);
            gridPagination.setGrid(propertyGroupGrid);
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
            vm.updateWatch = pubsub.subscribe("cc.property-group-radio", vm.updateRecords);
        };

        vm.isActive = function () {
            return SMDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                propertyGroupGrid.busy(true);
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId()
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
            propertyGroupGrid.busy(false);
            if (resp.records && resp.records.length > 0) {
                gridPagination.setData(resp.records).goToPage({
                    number: 0
                });

                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                SMDataModel.setProperties(resp.records);
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

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageSpendManagementProductAccess;
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.updateWatch();
            gridPagination.destroy();
            propertyGroupGrid.destroy();
            propertyGroupGridTransform.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }

            vm = undefined;
            $scope = undefined;
            gridPagination = undefined;
            propertyGroupGrid = undefined;
            propertyGroupGridTransform = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("SMPropertyGroupGridCtrl", [
            "$scope",
            "$filter",
            "SMPropertyGroupSvc",
            "rpGridModel",
            "SMPropertyGroupGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "spendManagementDataModel",
            "userDetailsModel",
            "routeSecurity",
            SMPropertyGroupGridCtrl
        ]);
})(angular);
