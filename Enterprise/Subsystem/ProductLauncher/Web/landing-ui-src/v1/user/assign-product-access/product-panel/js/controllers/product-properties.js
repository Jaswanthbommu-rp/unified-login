//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertiesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, pubsub, persona, productDataModel, userDetailsModel, security, configModel, syncMgr) {
        var vm = this,
            hasViewUserAccess,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "",
            activeProperties = [],
            inactiveProperties = [];

        vm.init = function () {
            vm.propertySelect = "property";
            vm.productId = 0;
            vm.activeProperties = activeProperties;
            vm.inactiveProperties = inactiveProperties;

            genericDataErrorReason = $filter("productPanelText")("panelError.generic");

            console.log('PROPERTY');
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
            vm.productPropertyWatch = $scope.$watch(vm.isActive, vm.loadData);
           //vm.productPropertyWatch = pubsub.subscribe("product.ProductPropertyData", vm.setData);


            // if (persona.isReady()) {
            //     vm.loadData();
            // }
            // else {
            //     vm.personaWatch = persona.subscribe(vm.loadData);
            // }

            pubsub.subscribe("ppanel.property-radio", vm.updatePropertyRecords);
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
        };

        vm.isUserHasManageProductAccess = function () {
            var productId = $scope.$parent.productId;
            switch (productId) {
                case "10" :
                    return !persona.data.hasProspectContactCenterProductAccess;
                    break;
                case "10" :
                    return !persona.data.hasManageClientPortalProductAccess;
                    break;
                default:
                    return false;

            }
        };

        vm.isReady = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.loadData = function () {
            var productId = $scope.$parent.productId;
            propertiesGrid.busy(false);
            var propData = syncMgr.getProductPropertiesData(productId);

            if (propData && propData.length > 0) {
                if (vm.hasViewOnlyAccess) {
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
                });

                propertiesGridPagination.setData(propData).goToPage({
                    number: 0
                });
            }

             return vm;
        };

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
            vm.clearProperties();
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
            if (vm.propertySelect === 'all') {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                productDataModel.setProperties(allPropertiesArray);
                productDataModel.setAllProperty(true);
            }
            else {
                productDataModel.setProperties(vm.dataReq.records);
                productDataModel.setAllProperty(false);
            }
        };

        vm.setAllProperties = function () {
            if (vm.propertySelect === 'all') {
                var allPropertiesArray = [];
                allPropertiesArray.push(-1);
                productDataModel.setProperties(allPropertiesArray);
            }
            else {
                productDataModel.setProperties(vm.dataReq.records);
            }
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.productPropertyWatch();
            propertiesGrid.destroy();
            propertiesGridTransform.destroy();
            propertiesGridPagination.destroy();
            propertiesGrid = undefined;
            propertiesGridTransform = undefined;
            propertiesGridPagination = undefined;

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
            ProductPropertiesGridCtrl
        ]);
})(angular);
