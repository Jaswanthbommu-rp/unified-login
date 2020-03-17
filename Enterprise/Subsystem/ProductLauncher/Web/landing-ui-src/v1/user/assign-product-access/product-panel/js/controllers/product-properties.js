//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertiesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, pubsub, persona, productDataModel, userDetailsModel, security, configModel, syncMgr, propertiesSvc, switchConfig) {
        var vm = this,
            hasViewUserAccess,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            activeProperties = [],
            inactiveProperties = [];

        vm.init = function () {
            vm.propertySelect = "property"; //property
            vm.productId = 0;
            vm.activeProperties = activeProperties;
            vm.inactiveProperties = inactiveProperties;

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");

            // console.log('PROPERTY');
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);

            vm.config = configModel.getGridConfig()[0];

            propertiesGrid.setConfig(vm.config);
            propertiesGridPagination.setGrid(propertiesGrid);
            $scope.propertiesGridPagination = propertiesGridPagination;
            propertiesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.radioconfig = configModel.getRadioConfig();
            vm.switchconfigs = configModel.getSwitchConfig();

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            // vm.productSelectedWatch = pubsub.subscribe("product.selectedProduct", vm.productSelected );
            vm.productPropertyWatch = $scope.$watch(vm.isActive, vm.loadData);
            //vm.productPropertyWatch = pubsub.subscribe("product.ProductPropertyData", vm.setData);

            pubsub.subscribe("ppanel.property-radio", vm.updatePropertyRecords);
            vm.gridAllWatch = propertiesGrid.subscribe("selectAll", vm.selectionAll);
            vm.gridSelectionWatch = propertiesGrid.subscribe("selectChange", vm.updateMultiSelectPropertyRecords);
            vm.filterData = propertiesGrid.subscribe("filterBy", vm.filter.bind(vm));
        };

        vm.productSelected = function (obj) {
            vm.productId = obj.productId;
            $scope.productId = obj.productId;
        };


        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };

        // vm.isUserHasManageProductAccess = function () {
        //     var productId = $scope.$parent.productId;
        //     logc("test", persona.data.hasProspectContactCenterProductAccess);
        //     switch (productId) {
        //     case "10":
        //         return persona.data.hasProspectContactCenterProductAccess;
        //     case "14":
        //         return persona.data.hasManageClientPortalProductAccess;
        //     default:
        //         return false;
        //     }
        // };

        vm.filter = function (filterBy) {
            if (vm.propertySelect === 'active') {
                vm.filteredPropertiesArray = $filter("filter")(vm.activeProperties, filterBy);
            }
            else if (vm.propertySelect === 'inactive') {
                vm.filteredPropertiesArray = $filter("filter")(vm.inactiveProperties, filterBy);
            }

            propertiesGridPagination.setData(vm.filteredPropertiesArray).goToPage({
                number: 0
            });
        };

        vm.isReady = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;

            propertiesGrid.busy(true);
            if (persona.isReady() && vm.isActive()) {
                var propertyData = syncMgr.getProductPropertiesData(productId);

                if (vm.switchconfigs !== undefined && vm.switchconfigs.length > 0) {
                    vm.switchconfigs[0].configData = switchConfig({
                        onChange: vm.selectionAll,
                        disabled: vm.hasViewOnlyAccess()
                    });
                }

                if (propertyData === undefined) {
                    propertiesGrid.busy(false);
                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        productId: productId
                    };

                    vm.dataPropReq = propertiesSvc.get(params, vm.setPropertyData);
                }
                else {
                    vm.loadGridData(productId);
                }
            }
            propertiesGrid.busy(false);
        };

        vm.setPropertyData = function (resp) {
            if (resp.records && resp.records.length > 0) {
                var pdata = syncMgr.setPropertyList(resp.records, $scope.$parent.productId);

                if (resp.additional && resp.additional.allProperties) {
                    syncMgr.updateProductAllProperties($scope.$parent.productId, true);
                }
                vm.loadGridData($scope.$parent.productId);
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

        vm.loadGridData = function (productId) {
            //var productId = $scope.$parent.productId;
            var propertySelect = "property";
            var propData = syncMgr.getProductPropertiesData(productId);

            if (propData && propData.length > 0) {
                if (vm.hasViewOnlyAccess()) {
                    propData.forEach(function (item) {
                        angular.extend(item, {
                            disabled: false,
                            radname: "property"
                        });
                        item.disabled = true;
                    });
                }

                propData.forEach(function (item) {
                    angular.extend(item, {
                        radname: "property",
                        productId: productId
                    });

                    if (item.active !== undefined && productId === 10) {
                        propertySelect = "active";
                        if (item.active == 'true') {
                            vm.activeProperties.push(item);
                            if (item.isAssigned) {
                                propertySelect = "active";
                            }
                        }
                        else {
                            vm.inactiveProperties.push(item);
                            if (item.isAssigned) {
                                propertySelect = "inactive";
                            }
                        }
                    }
                });

                if (syncMgr.isProductAllProperties(productId)) {
                    propertySelect = "all";
                }

                vm.propertySelect = propertySelect;
                //propertiesGridPagination.setData(propData).goToPage({number: 0});
                if (productId == "10") {
                    // vm.propertySelect = propertySelect;
                    if (propertySelect == "active") {
                        propertiesGridPagination.setData(vm.activeProperties).goToPage({
                            number: 0
                        });
                        vm.propertiesGrid.filtersModel.reset();
                    }
                    else if (propertySelect == "inactive") {
                        propertiesGridPagination.setData(vm.inactiveProperties).goToPage({
                            number: 0
                        });
                        vm.propertiesGrid.filtersModel.reset();
                    }
                }
                else {
                    propertiesGridPagination.setData(propData).goToPage({
                        number: 0
                    });
                }
            }

            return vm;
        };

        vm.selectionAll = function (bool) {
            vm.propertySelect = "property";
            if (bool) {
                vm.propertySelect = 'all';
            }
            var data = syncMgr.allPropertiesSync($scope.$parent.productId, bool);
            vm.resetProperties();
        };

        vm.updatePropertyRecords = function (record) {
            if (record){
                var propertiesData = syncMgr.selectedPropertySync(record.productId, record);
            }
        };

        vm.updateMultiSelectPropertyRecords = function (record) {
            if (record){
                var propertiesData = syncMgr.multiSelectedPropertySync(record.productId, record);
            }
        };

        vm.resetDataModel = function () {
            //vm.clearProperties();
            vm.resetProperties();
        };

        vm.resetProperties = function () {
            var allProperties = false;
            if (vm.propertySelect === 'all') {
                allProperties = true;
            }

            syncMgr.updateProductAllProperties($scope.$parent.productId, allProperties);

            if (vm.propertySelect == "active") {
                propertiesGridPagination.setData(vm.activeProperties).goToPage({
                    number: 0
                });
                vm.propertiesGrid.filtersModel.reset();
            }
            else if (vm.propertySelect == "inactive") {
                propertiesGridPagination.setData(vm.inactiveProperties).goToPage({
                    number: 0
                });
                vm.propertiesGrid.filtersModel.reset();
            }
        };


        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.productPropertyWatch();
            vm.gridAllWatch();
            vm.gridSelectionWatch();
            //  vm.productSelectedWatch();
            if (vm.dataPropReq) {
                vm.dataPropReq.$cancelRequest();
            }
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            propertiesGridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            propertiesGridPagination = undefined;
            vm.filteredPropertiesArray = [];
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductPropertiesGridCtrl", [
            "$scope",
            "$filter",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "pubsub",
            "personaDetails",
            "productPanelDataModel",
            "userDetailsModel",
            "routeSecurity",
            "ConfigModel",
            "productDataSyncManager",
            "productPropertiesSvc",
            "rpSwitchConfig",
            ProductPropertiesGridCtrl
        ]);
})(angular);
