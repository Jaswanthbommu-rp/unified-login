//  Property Groups Grid Tab Controller

(function (angular, undefined) {
    "use strict";

    function ProductNotificationGridCtrl($scope, $filter, dataSvc, security, persona, syncMgr, productDataModel, userDetailsModel) {
        var vm = this;

        vm.init = function () {
            vm.frontDesk = false;
            vm.amenity = false;
            vm.serviceReq = false;

            vm.activeWatch = $scope.$watch(vm.isActive, vm.loadData);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.isActive = function () {
            return productDataModel.isPropertyGridActive();
        };

        vm.hasViewOnlyAccess = function () {
            return security.isAllowed("viewUser") || syncMgr.isUserHasManageProductAccess($scope.$parent.productId);
        };


        vm.loadData = function () {
            var productId = $scope.$parent.productId;

            if (persona.isReady() && vm.isActive()) {
                var propertyData = syncMgr.getProductPropertiesData(productId);

                if (propertyData === undefined) {

                    var params = {
                        userPersonaId: userDetailsModel.getPersonaId(),
                        editorPersonaId: persona.getId()
                    };

                    vm.dataPropReq = dataSvc.get(params, vm.setPropertyGroupData);
                }
                else {
                    vm.loadGridData(productId);
                }
            }
        };

        vm.loadGridData = function (productId) {
            // pmgGrid.busy(false);

            // var propData = syncMgr.getMessageGroupMap(productId);

            // if (propData && propData.length > 0) {
            //     if (vm.hasViewOnlyAccess()) {
            //         propData.forEach(function (item) {
            //             angular.extend(item, {
            //                 disabled: false,
            //                 radname: "messageGroup"
            //             });
            //             item.disabled = true;
            //         });
            //     }

            //     propData.forEach(function (item) {
            //         angular.extend(item, {
            //             radname: "messageGroup",
            //             productId: productId,
            //             originalProperty: item.isAssigned
            //         });

            //     });

            //     pmgGridPagination.setData(propData).goToPage({
            //         number: 0
            //     });
            // }

            return vm;
        };

        // vm.setPropertyGroupData = function (resp) {
        //     pmgGrid.busy(false);
        //     if (resp.records && resp.records.length) {
        //         var pdata = syncMgr.setMessageGroupList(resp.records, $scope.$parent.productId);
        //         vm.loadGridData($scope.$parent.productId);
        //     }

        //     if (resp.isError) {
        //         vm.isPropertyGroupsError = true;
        //     }
        // };



        vm.destroy = function () {
            vm.destWatch();
            vm.activeWatch();


            //vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductNotificationGridCtrl", [
            "$scope",
            "$filter",
            "resPortNotificationsSvc",
            "routeSecurity",
            "personaDetails",
            "productDataSyncManager",
            "productPanelDataModel",
            "userDetailsModel",
            ProductNotificationGridCtrl
        ]);
})(angular);
