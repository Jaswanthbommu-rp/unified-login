//  Properties Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductPropertiesGridCtrl($scope, $filter, gridModel, gridTransformSvc, gridPaginationModel, pubsub, persona, productDataModel, userDetailsModel, security, configModel, syncMgr) {
        var vm = this,
            hasViewUserAccess,
            propertiesGrid = gridModel(),
            propertiesGridTransform = gridTransformSvc(),
            propertiesGridPagination = gridPaginationModel(),
            genericDataErrorReason = "";

        vm.init = function () {
            vm.propertySelect = "property";
            vm.productId = "";
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

            vm.radioconfig = configModel.getRadioConfig()[0];
            logc("cnfg for radio prop" ,vm.radioconfig);

            vm.personaWatch = angular.noop;
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
           // vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
           vm.productPropertyWatch = pubsub.subscribe("product.ProductPropertyData", vm.setData);


            // if (persona.isReady()) {
            //     vm.loadData();
            // }
            // else {
            //     vm.personaWatch = persona.subscribe(vm.loadData);
            // }

            //pubsub.subscribe("cp.property-radio", vm.updateRecords);
        };


        vm.isUserHasManageProductAccess = function () {
            return !persona.data.hasManageClientPortalProductAccess;
        };

        vm.isActive = function () {
            return true;//productDataModel.isActive();
        };

        // vm.loadData = function () {
        //     if (persona.isReady() && vm.isActive()) {
        //         vm.hasViewUserAccess = security.isAllowed("viewUser") || vm.isUserHasManageProductAccess();
        //         propertiesGrid.busy(true);
        //         var params = {
        //             userPersonaId: userDetailsModel.getPersonaId(),
        //             editorPersonaId: persona.getId()
        //         };

        //         vm.activeWatch();
        //         vm.personaWatch();
        //         vm.dataReq = dataSvc.get(params, vm.setData);
        //     }
        // };

        vm.updateRecords = function (record) {
            var propertiesData = productDataModel.getProperties();

            propertiesData.forEach(function (item) {
                item.isAssigned = item.id == record.id;
            });
        };

        vm.setData = function (productId) {
            logc(vm.propertiesGrid);
            vm.propertiesGrid.busy(false);
            var propData = syncMgr.getProductPropertiesData(productId);
            logc("propData", propData);
            if (propData && propData.length > 0) {
                vm.productId = productId;
                if (security.isAllowed("viewUser") || vm.isUserHasManageProductAccess()) {
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
                            radname: "property"
                        });
                });


                propertiesGridPagination.setData(propData).goToPage({
                    number: 0
                });

                // if (resp.additional) {
                //     var allProperties = resp.additional.allProperties;

                //     if (allProperties) {
                //         vm.propertySelect = "all";
                //         productDataModel.setAllProperty(true);
                //     }
                //     else {
                //         vm.propertySelect = "property";
                //         productDataModel.setAllProperty(false);
                //     }
                // }
                logc("vm.propertiesGrid",vm.propertiesGrid);
                productDataModel.setProperties(propData);
            }
            // if (resp.isError) {
            //     vm.isDataError = true;
            //     if (resp.errorReason !== "") {
            //         vm.dataErrorReason = resp.errorReason;
            //     }
            //     else {
            //         vm.dataErrorReason = genericDataErrorReason;
            //     }
            // }


        };

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
