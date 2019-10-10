//  Areas Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function DepositAltAreasGridCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, daDataModel, userDetailsModel, security, pubsub) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
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
            vm.gridWatch = pubsub.subscribe("DA.areas", vm.gridUpdate);

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }
        };

        vm.gridUpdate = function () {            
            vm.grid.updateSelected();
        };

        vm.isActive = function () {
            return daDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && vm.isActive()) {
                grid.busy(true);
               
                var params = {
                    productType: "DepositAlternative",                    
                    editorPersonaId: persona.getId(),
                    subjectPersonaId: userDetailsModel.getPersonaId(),                    
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            
            vm.records = [];
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    resp.records.forEach(function (item) {
                        angular.extend(item, {
                            disableSelection: false
                        });
                        item.disableSelection = true;
                    });
                }

                resp.records.map(function (area) {     
                    if(area.groupType === 'area'){         
                        vm.records.push(area);
                    }
                });

                
                gridPagination.setData(vm.records).goToPage({
                    number: 0
                });

                daDataModel.setAreas(vm.records);
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
            return !persona.data.hasManageDepositAlternativeProductAccess;
        };


        vm.destroy = function () {
            vm.personaWatch();
            vm.activeWatch();
            vm.destWatch();
            vm.gridWatch();
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
        .controller("DepositAltAreasGridCtrl", [
            "$scope",
            "$filter",
            "DAAreasSvc",
            "rpGridModel",
            "daAreasGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "depositAlternativeProductAccessModel",
            "userDetailsModel",
            "routeSecurity",
            "pubsub",
            DepositAltAreasGridCtrl
        ]);
})(angular);