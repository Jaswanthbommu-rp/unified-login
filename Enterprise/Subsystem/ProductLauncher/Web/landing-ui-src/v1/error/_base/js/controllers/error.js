//  Error Controller

(function (angular, undefined) {
    "use strict";

    function ErrorCtrl($scope, appLayout) {
        var vm = this;

        vm.init = function () {
            appLayout.hide("appSubheader");
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.destroy = function () {
            appLayout.show("appSubheader");
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ErrorCtrl", ["$scope", "appLayoutModel", ErrorCtrl]);
})(angular);
