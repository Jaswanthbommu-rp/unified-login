//  rights PropertyManagement Controller

(function(angular, undefined) {
    "use strict";

    function AcctRightsCtrl(
        $scope,
        $filter,
        model,
        acctRightsGridConfig,
        acctRightsGridActions,
        prodConfig,
        productsMenu,
        pubsub,
        $timeout
    ) {

        var vm = this;

        vm.init = function() {

            acctRightsGridConfig.setSrc(vm);
            acctRightsGridActions.setSrc(vm);

            vm.model = model;
            prodConfig.setMethodsSrc(vm);

            vm.prodConfig = prodConfig;
            productsMenu.getProdData();
            vm.model.initWatch();
            vm.model.initGrid();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

            vm.smWatch = pubsub.subscribe("acctSettings.setSelMenu", vm.setSelMenu);

        };

        vm.setMenuData = function() {
            prodConfig.setOptionsFilter("optionsData", vm.optionsFilterVal);
            vm.filterMenuVal = productsMenu.getSelProduct();
        };


        vm.setSelMenu = function() {
            vm.filterMenuVal = productsMenu.getSelProduct();
        };

        vm.menuChange = function(value) {
            productsMenu.setSelProduct(value);
            pubsub.publish("acctSettings.productChange");
        };

        vm.assignRoles = function(record) {
            vm.model.assignRolestoRights(record);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.smWatch();
            vm.model.reset();
            vm = undefined;
        };


        vm.init();
    }

    angular
        .module("settings")
        .controller("AcctRightsCtrl", [
            "$scope",
            "$filter",
            "rolAndRhtAcctRightsModel",
            "acctRightsGridConfig",
            "acctRightsGridActions",
            "acctProductsConfig",
            "acctProductsSelectMenu",
            "pubsub",
            "$timeout",
            AcctRightsCtrl
        ]);
})(angular);