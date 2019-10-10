//  Product Settings Controller

(function (angular) {
    "use strict";

    function ProductCtrl($state) {
        var vm = this;

        vm.init = function () {
        };

        vm.destroy = function () {
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ProductCtrl", [
            "$state",
        	ProductCtrl
        ]);
})(angular);