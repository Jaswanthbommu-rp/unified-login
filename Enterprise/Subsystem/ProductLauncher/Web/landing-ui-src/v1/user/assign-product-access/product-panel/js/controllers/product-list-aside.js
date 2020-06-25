//  Rights List Controller

(function (angular, undefined) {
    "use strict";

    function ProductPanelListAsideCtrl($scope, $filter, aside, dataSvc, groupSvc, syncMgr, gridModel, gridTransformSvc, gridPaginationModel, listAsideModel, persona) {
        var vm = this,
            asideGrid = gridModel(),
            asidegridTransform = gridTransformSvc(),
            asidegridPagination = gridPaginationModel(),
            isBtnFooterRequired;

        vm.init = function () {
            vm.subtitle = listAsideModel.getName();
            vm.tabName = listAsideModel.getTabName();
            vm.productId = listAsideModel.getProductID();
            vm.roleType = listAsideModel.getRoleType();
            vm.asideGrid = asideGrid;
            vm.properteiesData = {};
            vm.propertyRecords = listAsideModel.getSelectedPropertyRoleData();
            
            asidegridTransform.watch(asideGrid);
            vm.isBtnFooterRequired = listAsideModel.FooterRequired(vm.productId);
            syncMgr.setAsidePropertyList(listAsideModel.getSelectedPropertyRoleData(), vm.productId);
            var configTab = "";
            if (vm.tabName.toLowerCase() === "property") {
                if(vm.productId == 44){
                    configTab = "Properties";
                    vm.title = "Assigned Entities";
                    vm.subtitle= persona.data.organization.name;
                }else{
                    configTab = "Properties";
                    vm.title = "Property Details";
                }              
            }
            else if (vm.tabName.toLowerCase() === "propertygroup") {
                configTab = "PropertyGroup";
                vm.title = "Property Group Details";
            }
            else if (vm.tabName.toLowerCase() == "role") {
                if(vm.productId == 20){
                    vm.subtitle= persona.data.organization.name;
                    configTab = vm.roleType.replace(/ /g, "");
                    vm.title = "Assign " + vm.roleType;
                }
                else{
                    configTab = "Roles";
                    vm.title = "Role Details";
                }
            }

            vm.asideConfig = syncMgr.getProductAsideGridConfig(vm.productId, configTab);
            //gridConfig.getListAsideConfig()[0];
            //vm.title = syncMgr.getProductAsideGridName(vm.productId, configTab);
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
            vm.gridAllWatch = asideGrid.subscribe("selectAll", vm.selectAllProperties);
            vm.gridSelectionWatch = asideGrid.subscribe("selectChange", vm.selectionChange);
            vm.filterData = asideGrid.subscribe("filterBy", vm.filter.bind(vm));
            return vm;
        };

        vm.filter = function(filterBy){
            vm.filteredRecords = $filter("filter")(vm.propertyRecords.propertiesList, filterBy);
        };

        vm.loadData = function () {
            asideGrid.busy(true);
            var productId = listAsideModel.getProductID();
            var assignedToRoleOnly = false;
            var aoFamilyProduct = false;
            var params = "";
            if (productId === 1) {
                assignedToRoleOnly = true;
            }

            if (productId == "29" || productId == "30" || productId == "31" || productId == "32" ||
                productId == "51" || productId == "52" || productId == "53" || productId == "54") {
                aoFamilyProduct = true;
            }

            if (aoFamilyProduct) {
                params = {
                    editorPersonaId: persona.getId(),
                    userPersonaId: "0",
                    productId: productId,
                    propertyGroupId: listAsideModel.getListID()
                };

                vm.dataReq = groupSvc.get(params, vm.setData);
            }else if(productId == "20" || productId == "44"){
                vm.properteiesData.records = vm.propertyRecords.propertiesList;
                vm.setData(vm.properteiesData);
            }
            else {
                params = {
                    editorPersonaId: persona.getId(),
                    roleId: listAsideModel.getListID(),
                    productId: productId,
                    assignedToRoleOnly: assignedToRoleOnly,
                    partyId: persona.data.organization.partyId,
                };

                vm.dataReq = dataSvc.get(params, vm.setData);
            }

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
        
        vm.update = function(){
            aside.hide();
        };

        vm.selectAllProperties = function (val) {
            if(vm.filteredRecords !== undefined){
                syncMgr.updateAllFilterAsideProperties(vm.productId, vm.filteredRecords, val);
            }
            else{
                syncMgr.updateAllFilterAsideProperties(vm.productId, vm.propertyRecords.propertiesList, val);
            }
        };

        vm.selectionChange = function (record) {
            if (record) {
                syncMgr.selectedAsidePropertySync(vm.productId);
            }
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
            "$filter",
            "productPanelListAside",
            "productRoleRightsSvc",
            "productGroupPropertiesSvc",
            "productDataSyncManager",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "productPanelListAsideModel",
            "personaDetails",
            ProductPanelListAsideCtrl
        ]);
})(angular);
