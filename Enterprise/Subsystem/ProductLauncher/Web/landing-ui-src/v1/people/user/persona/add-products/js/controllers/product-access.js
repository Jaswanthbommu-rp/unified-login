
(function (angular) {
    "use strict";

    function ProductAccessCtrl($scope, personaProducts, rpWatchList) {
        var vm = this;

        vm.init = function () {
            vm.watchList = rpWatchList();
            vm.watchList.add($scope.$on("$destroy", vm.destroy));

            vm.personaProducts = personaProducts;
            vm.personaProducts.openFirstProduct();
        };

        vm.destroy = function () {
            vm.watchList.destroy();
            vm.watchList = undefined;

            vm.personaProducts = undefined;
            vm = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductAccessCtrl", [
            "$scope",
            "personaProducts",
            "rpWatchList",
            ProductAccessCtrl
        ]);
        
})(angular);
