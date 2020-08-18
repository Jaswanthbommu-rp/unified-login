//  Product Info Icon Controller

(function (angular, undefined) {
    "use strict";

    function ProductInfoIconCtrl($scope, aside, innerAside, syncMgr, listModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function (record, tabname) {
            logc("console.info()",record);
            listModel.setName(record.name);
            if(record.tabname !== undefined){
                tabname = record.tabname;
            }
            
            listModel.setTabName(tabname);
            listModel.setListID(record.id);
            listModel.setProductID(record.productId);
            if(record.productId === 20) {
                 listModel.setRoleType(record.roletype);
             }
            if(tabname === 'EntityGroup'){
                listModel.setSelectedPropertyRoleData(record);
                innerAside.show();
            } 
            // if(tabname === 'group'){
            //     listModel.setSelectedGroupRoleData(record);
            //     aside.show();
            // }
            else{
                listModel.setSelectedPropertyRoleData(record);
                aside.show();
            }
            
        };

        vm.destroy = function () {
            vm.destWatch();
            aside = undefined;
            innerAside = undefined;
            listModel = undefined;
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductInfoIconCtrl", [
            "$scope",
            "productPanelListAside",
            "productPanelListInnerAside",
            "productDataSyncManager",
            "productPanelListAsideModel",
            ProductInfoIconCtrl
        ]);
})(angular);
