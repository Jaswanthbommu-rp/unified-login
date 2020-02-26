//  Access Not Defined Controller

(function (angular, undefined) {
    "use strict";

    function NewProductAccessCtrl($scope, model, data, tabsMenu, pubsub) {
        var vm = this;

        vm.init = function () {
            console.log("controller init");
            vm.model = model;
            vm.model.setData(data);
            console.log("tabsMenu", tabsMenu);
            vm.pageTitle = vm.model.getPageTitle();
            vm.productSelectedWatch = pubsub.subscribe("selectedProduct", vm.productSelected );
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.productSelected = function (obj) {
            logc("selected Product", obj);
            vm.pageTitle = vm.model.getPageTitle() + " , Product Selected : " + obj.productId;
        };

        vm.destroy = function () {
            model.reset();
            vm.productAccessWatch();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("NewProductAccessCtrl", [
            "$scope",            
            "newProductAccessModel",
            "DataModelNew",
            "tabsMenu",
            "pubsub",
            NewProductAccessCtrl
        ]);
})(angular);
