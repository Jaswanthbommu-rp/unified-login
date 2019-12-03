//  BusinessIntelligence Properties Controller

(function (angular, undefined) {
    "use strict";

    function BusinessIntelligencePropertiesCtrl($scope, $filter, dataSvc, gridModel, gridConfig, gridTransformSvc, gridPaginationModel, persona, BIDataModel, userDetailsModel, sync, pubsub, switchConfig, security) {
        var vm = this,
            grid = gridModel(),
            gridTransform = gridTransformSvc(),
            gridPagination = gridPaginationModel(),
            userLoginName = "",
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
            vm.gridAllWatch = grid.subscribe("selectAll", vm.selectAllProperties);
            vm.updateGridWatch = pubsub.subscribe("businessIntelligence.updateGrids", vm.updateGrid);
            
        };

        vm.updateGrid = function () {
            vm.grid.updateSelected();
        };


        vm.isActive = function () {
            return BIDataModel.isActive();
        };

        vm.loadData = function () {
            if (persona.isReady() && BIDataModel.isActive()) {
                grid.busy(true);
                var companyData = BIDataModel.getCompanies();
                var params = {
                    userPersonaId: userDetailsModel.getPersonaId(),
                    editorPersonaId: persona.getId(),
                    productName: "BI",
                    userLoginName: userDetailsModel.getLoginName() === undefined ? userLoginName : userDetailsModel.getLoginName() 
                };

                vm.activeWatch();
                vm.personaWatch();
                vm.dataReq = dataSvc.get(params, vm.setData);
            }
        };

        vm.setData = function (resp) {
            grid.busy(false);
            if (resp.records && resp.records.length > 0) {
                var propertyData = resp.records[0];
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
                    vm.setViewUserState(propertyData);
                }
                else {
                    gridPagination.setData(propertyData.properties).goToPage({
                        number: 0
                    });

                    if (resp.additional && resp.additional.allProperties) {
                        vm.allProperties = resp.additional.allProperties;
                        vm.setAllProperties(true);
                    }
                    else {
                        BIDataModel.setProperties(propertyData);
                    }
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

        vm.setAllProperties = function (val) {
            if (val) {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                BIDataModel.setProperties(allPropertiesArray);

                //clear selections, if theres any
                vm.grid.selectAll(false);
                vm.grid.updateSelected();
            }
            else {
                BIDataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.setViewUserState = function (data) {
            data.properties.forEach(function (item) {
                angular.extend(item, {
                    disableSelection: false
                });
                item.disableSelection = true;
            });
            vm.allProperties = false;

            gridPagination.setData(data.properties).goToPage({
                number: 0
            });
            BIDataModel.setProperties(data);
        };

        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageAssetOptimizationProductAccess;
        };

        vm.selectAllProperties = function (val) {
            BIDataModel.setAllPropertiesData(vm.dataReq.records[0].properties, val);
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.gridAllWatch();
            vm.updateGridWatch();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            grid.destroy();
            gridTransform.destroy();
            gridPagination.destroy();
            grid = undefined;
            gridTransform = undefined;
            gridPagination = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("BusinessIntelligencePropertiesCtrl", [
            "$scope",
            "$filter",
            "AssetOptimizationCompanyPropertyListSvc",
            "rpGridModel",
            "businessIntelligencePropertiesGridConfig",
            "rpGridTransform",
            "rpGridPaginationModel",
            "personaDetails",
            "businessIntelligenceDataModel",
            "userDetailsModel",
            "syncManager",
            "pubsub",
            "rpSwitchConfig",
            "routeSecurity",
            BusinessIntelligencePropertiesCtrl
        ]);
})(angular);
