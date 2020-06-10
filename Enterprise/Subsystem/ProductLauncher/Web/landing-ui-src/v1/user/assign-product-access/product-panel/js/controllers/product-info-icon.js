//  Product Info Icon Controller

(function (angular, undefined) {
    "use strict";

    function ProductInfoIconCtrl($scope, aside, syncMgr, listModel) {
        var vm = this;

        vm.init = function () {
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.showAside = function (record) {
            logc("console.info()",record);
            listModel.setName(record.name);
            listModel.setTabName(record.radname);
            listModel.setListID(record.id);
            listModel.setProductID(record.productId);
            listModel.setSelectedPropertyRoleData(record);
            aside.show();
        };

        vm.destroy = function () {
            vm.destWatch();
            aside = undefined;
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
            "productDataSyncManager",
            "productPanelListAsideModel",
            ProductInfoIconCtrl
        ]);
})(angular);
