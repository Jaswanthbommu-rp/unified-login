//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertiesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, pubsub, persona, productDataModel, userDetailsModel, security, configModel, syncMgr, propertiesSvc) {
        var vm = this,
            hasViewUserAccess,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            activeProperties = [],
            inactiveProperties = [];

        vm.init = function () {
            vm.propertySelect = "property";//property
            vm.productId = 0;
            vm.activeProperties = activeProperties;
            vm.inactiveProperties = inactiveProperties;

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");

           // console.log('PROPERTY');
            vm.propertiesGrid = propertiesGrid;
            propertiesGridTransform.watch(propertiesGrid);

            console.log("configModel.getConfig()", configModel.getConfig("Properties"));
            // vm.config = configModel.getGridConfig()[0];
            vm.config = configModel.getConfig("Properties");
            //logc("configModel.getGridConfig()[0]", configModel.getGridConfig());

            propertiesGrid.setConfig(vm.config.gridConfig);
            propertiesGridPagination.setGrid(propertiesGrid);
            $scope.propertiesGridPagination = propertiesGridPagination;
            propertiesGridPagination.setConfig({
                recordsPerPage: 25
            });

            vm.radioconfig = vm.config.radioConfig;//configModel.getRadioConfig();
            vm.switchconfigs = vm.config.switchConfig; //configModel.getSwitchConfig();

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
           // vm.productSelectedWatch = pubsub.subscribe("product.selectedProduct", vm.productSelected );
            vm.productPropertyWatch = $scope.$watch(vm.isActive, vm.loadData);
           //vm.productPropertyWatch = pubsub.subscribe("product.ProductPropertyData", vm.setData);

            pubsub.subscribe("ppanel.property-radio", vm.updatePropertyRecords);
            vm.filterData = propertiesGrid.subscribe("filterBy", vm.filter.bind(vm));
        };

         vm.productSelected = function (obj) {
            vm.productId = obj.productId;
            $scope.productId = obj.productId;
        };


        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
        };

        vm.isUserHasManageProductAccess = function () {
            var productId = $scope.$parent.productId;
            logc("test", persona.data.hasProspectContactCenterProductAccess);
            switch (productId) {
                case "10" :
                    return persona.data.hasProspectContactCenterProductAccess;
                case "14" :
                    return persona.data.hasManageClientPortalProductAccess;
                default:
                    return false;
            }
        };

        vm.filter = function(filterBy){
            logc("filterBy", filterBy);
            if(vm.propertySelect === 'active') {
                vm.filteredPropertiesArray = $filter("filter")(vm.activeProperties, filterBy);
            }
            else if(vm.propertySelect === 'inactive') {
                vm.filteredPropertiesArray = $filter("filter")(vm.inactiveProperties, filterBy);
            }
logc("vm.filteredPropertiesArray", vm.filteredPropertiesArray);
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
            if (persona.isReady() && vm.isActive()) {
                propertiesGrid.busy(true);
                  var propertyData = syncMgr.getProductPropertiesData(productId);
                 // logc("propertyData",propertyData,productId);
                  if (propertyData === undefined){
                    // propertiesGrid.busy(false);
                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId(),
                        productId: productId
                    };

                    vm.dataPropReq = propertiesSvc.get(params, vm.setPropertyData);
                  }
                  else {
                    //syncMgr.setPropertyGridActive(true);
                    vm.loadGridData(productId);
                  }
            }
        };

        vm.setPropertyData = function (resp) {

            //var productId = $scope.$parent.productId;
            if (resp.records && resp.records.length > 0){
               // logc("setPropertyData",resp.records, vm.productId);
                var pdata = syncMgr.setPropertyList(resp.records, $scope.$parent.productId);
                //syncMgr.setPropertyGridActive(true);
                if (resp.additional && resp.additional.allProperties) {
                    syncMgr.updateProductAllProperties($scope.$parent.productId, true);
                }
                vm.loadGridData($scope.$parent.productId);
             }
             propertiesGrid.busy(false);
        };

        vm.loadGridData = function (productId) {
            //var productId = $scope.$parent.productId;
            var propertySelect = "property";
            var propData = syncMgr.getProductPropertiesData(productId);

            if (propData && propData.length > 0) {
                // if (vm.hasViewOnlyAccess) {
                //     propData.forEach(function (item) {
                //         angular.extend(item, {
                //             disabled: false,
                //             radname: "property"
                //         });
                //         item.disabled = true;
                //     });
                // }

                propData.forEach(function (item) {
                    angular.extend(item, {
                        radname: "property",
                        productId: productId
                    });

                    if (item.active !== undefined && productId === 10){
                        propertySelect = "active";
                        if (item.active == 'true') {
                            vm.activeProperties.push(item);
                            if(item.isAssigned) {
                                propertySelect = "active";
                            }
                        }
                        else {
                            vm.inactiveProperties.push(item);
                            if(item.isAssigned) {
                                propertySelect = "inactive";
                            }
                        }
                    }
                });

                if (syncMgr.isProductAllProperties(productId)){
                    propertySelect = "all";
                }

                vm.propertySelect = propertySelect;
                //propertiesGridPagination.setData(propData).goToPage({number: 0});
                if (productId == "10")
                {
                   // vm.propertySelect = propertySelect;
                   logc("test data", propertySelect, vm.activeProperties);
                    if(propertySelect == "active") {
                        propertiesGridPagination.setData(vm.activeProperties).goToPage({
                            number: 0
                        });
                        vm.propertiesGrid.filtersModel.reset();
                    }
                    else if(propertySelect == "inactive") {
                        propertiesGridPagination.setData(vm.inactiveProperties).goToPage({
                            number: 0
                        });
                        vm.propertiesGrid.filtersModel.reset();
                    }
                }
                else {
                    propertiesGridPagination.setData(propData).goToPage({number: 0});
                }
            }
            logc("vmprop",vm, propertiesGridPagination);
            return vm;
        };

//         vm.loadDataxxxx = function () {
//             logc("vm.isActive", vm.isActive());
//             var productId = $scope.$parent.productId;
//             propertiesGrid.busy(false);
//             var propData = syncMgr.getProductPropertiesData(productId);

// logc("propData", propData);
// logc("vm.hasViewOnlyAccess", vm.hasViewOnlyAccess());
//             if (productId == "10")
//             {
//                 vm.propertySelect = "active";
//             }

//             if (propData && propData.length > 0) {
//                 // if (vm.hasViewOnlyAccess) {
//                 //     propData.forEach(function (item) {
//                 //         angular.extend(item, {
//                 //             disabled: false,
//                 //             radname: "property"
//                 //         });
//                 //         item.disabled = true;
//                 //     });
//                 // }

//                 propData.forEach(function (item) {
//                     angular.extend(item, {
//                         radname: "property",
//                         productId: productId
//                     });

//                     // if (item.active !== undefined && item.active == 'true') {
//                     //     vm.activeProperties.push(item);
//                     //     // if(property.isAssigned) {
//                     //     //     vm.propertySelect = "active";
//                     //     // }
//                     // }
//                     // else {
//                     //     vm.inactiveProperties.push(item);
//                     //     // if(property.isAssigned) {
//                     //     //     vm.propertySelect = "inactive";
//                     //     // }
//                     // }

//                 });

//                 propertiesGridPagination.setData(propData).goToPage({
//                     number: 0
//                 });
//             }

//              return vm;
//         };

        vm.updatePropertyRecords = function (record) {
            var propertiesData = syncMgr.selectedPropertySync(record.productId, record);
            logc("propertiesData",propertiesData);
        };

        // vm.setData = function (productId) {
        //    // vm.productId = productId;
        //     //logc("$scope.propertiesGridPagination",$scope.propertiesGridPagination);
        //     vm.propertiesGrid.busy(false);
        //     var propData = syncMgr.getProductPropertiesData(productId);

        //     if (propData && propData.length > 0) {
        //         if (security.isAllowed("viewUser")) {
        //             propData.forEach(function (item) {
        //                 angular.extend(item, {
        //                     disabled: false,
        //                     radname: "property"
        //                 });
        //                 item.disabled = true;
        //             });
        //         }

        //         propData.forEach(function (item) {
        //                 angular.extend(item, {
        //                     radname: "property",
        //                     productId: productId
        //                 });
        //         });

        //        // propertiesGridPagination.setGrid(propertiesGrid);
        //         propertiesGridPagination.setData(propData).goToPage({
        //             number: 0
        //         });

        //         // if (resp.additional) {
        //         //     var allProperties = resp.additional.allProperties;

        //         //     if (allProperties) {
        //         //         vm.propertySelect = "all";
        //         //         productDataModel.setAllProperty(true);
        //         //     }
        //         //     else {
        //         //         vm.propertySelect = "property";
        //         //         productDataModel.setAllProperty(false);
        //         //     }
        //         // }

        //         //productDataModel.setProperties(propData);

        //     }
        //     // if (resp.isError) {
        //     //     vm.isDataError = true;
        //     //     if (resp.errorReason !== "") {
        //     //         vm.dataErrorReason = resp.errorReason;
        //     //     }
        //     //     else {
        //     //         vm.dataErrorReason = genericDataErrorReason;
        //     //     }
        //     // }

        //      return vm;
        // };

        vm.resetDataModel = function () {
            //vm.clearProperties();
            vm.resetProperties();
        };

        vm.clearProperties = function () {
            vm.dataReq.records.forEach(function (property) {
                if (property.isAssigned) {
                    property.isAssigned = false;
                }
            });
        };

        vm.resetProperties = function () {
            logc("vm.propertySelect", vm.propertySelect);
            var allProperties = false;
            if (vm.propertySelect === 'all') {
                allProperties = true;
            }
            syncMgr.updateProductAllProperties($scope.$parent.productId, allProperties);

            if(vm.propertySelect == "active") {
                propertiesGridPagination.setData(vm.activeProperties).goToPage({
                    number: 0
                });
                vm.propertiesGrid.filtersModel.reset();
            }
            else if(vm.propertySelect == "inactive") {
                propertiesGridPagination.setData(vm.inactiveProperties).goToPage({
                    number: 0
                });
                vm.propertiesGrid.filtersModel.reset();
            }
        };

        // vm.setAllProperties = function () {
        //     if (vm.propertySelect === 'all') {
        //         var allPropertiesArray = [];
        //         allPropertiesArray.push(-1);
        //         productDataModel.setProperties(allPropertiesArray);
        //     }
        //     else {
        //         productDataModel.setProperties(vm.dataReq.records);
        //     }
        // };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.productPropertyWatch();
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
            ProductPropertiesGridCtrl
        ]);
})(angular);
