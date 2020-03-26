//  Rights List Controller

(function (angular, undefined) {
    "use strict";

    function ProductPanelListAsideCtrl($scope, aside, dataSvc, syncMgr, gridModel, gridTransformSvc, gridPaginationModel, listAsideModel, persona) {
        var vm = this,
            asideGrid = gridModel(),
            asidegridTransform = gridTransformSvc(),
            asidegridPagination = gridPaginationModel();

        vm.init = function () {
            vm.subtitle = listAsideModel.getName();
            vm.tabName = listAsideModel.getTabName();
            vm.productId = listAsideModel.getProductID();
            vm.asideGrid = asideGrid;
            asidegridTransform.watch(asideGrid);

            var configTab = "";
            if (vm.tabName == "property"){
                configTab = "Properties";
            }
            else  if (vm.tabName == "role"){
                configTab = "Roles";
            }

            vm.asideConfig = syncMgr.getProductAsideGridConfig(vm.productId, configTab);
            //gridConfig.getListAsideConfig()[0];
            vm.title = syncMgr.getProductAsideGridName(vm.productId, configTab);
            //gridConfig.getListAsideDisplayName();

            asideGrid.setConfig(vm.asideConfig);
            asidegridPagination.setGrid(asideGrid);
            $scope.asidegridPagination = asidegridPagination;

            asidegridPagination.setConfig({
                recordsPerPage: 10
            });

            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                vm.loadData();
            }
            else {
                vm.personaWatch = persona.subscribe(vm.loadData);
            }

            vm.destWatch = $scope.$on("$destroy", vm.destroy);
            return vm;
        };

        vm.loadData = function () {
            asideGrid.busy(true);
            var productId = listAsideModel.getProductID();
            var assignedToRoleOnly = false;
            if (productId === 1) {
                assignedToRoleOnly = true;
            }
            var params = {
                editorPersonaId: persona.getId(),
                roleId: listAsideModel.getListID(),
                productId: productId,
                assignedToRoleOnly: assignedToRoleOnly,
                partyId: persona.data.organization.partyId,
            };

            vm.dataReq = dataSvc.get(params, vm.setData);
        };

        vm.setData = function (resp) {
            asideGrid.busy(false);
            asidegridPagination.setData(resp.records).goToPage({
                number: 0
            });
        };

        vm.cancel = function () {
            aside.hide();
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            asideGrid.destroy();
            asidegridTransform.destroy();
            asidegridPagination.destroy();
            if (vm.dataReq) {
                vm.dataReq.$cancelRequest();
            }
            vm = undefined;
            $scope = undefined;
            asideGrid = undefined;
            asidegridTransform = undefined;
            asidegridPagination = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductPanelListAsideCtrl", [
            "$scope",
            "productPanelListAside",
            "productRightsSvc",
            //"ConfigModel",
            "productDataSyncManager",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "productPanelListAsideModel",
            "personaDetails",
            ProductPanelListAsideCtrl
        ]);
})(angular);
