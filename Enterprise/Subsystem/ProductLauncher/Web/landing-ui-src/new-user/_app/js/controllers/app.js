//  App Controller

(function (angular) {
    "use strict";

    function AppCtrl() {
        var vm = this;

        vm.init = function () {

        };

        vm.destroy = function () {

        };

        vm.init();
    }

    angular
        .module("new-user")
        .controller('AppCtrl', [AppCtrl]);
})(angular);
