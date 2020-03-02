//  Assign Product Access Controller

(function (angular, undefined) {
    "use strict";

    function AssignProductAccessCtrl($scope, model, templates, productDataSync) {
        var vm = this;

        vm.init = function () {
            vm.active = {};
            vm.list = templates.getList();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.destroy = function () {
            vm.destWatch();
            productDataSync.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("AssignProductAccessCtrl", [
            "$scope",
            "assignProductAccessModel",
            "productAccessTemplates",
            "productDataSyncManager",
            AssignProductAccessCtrl
        ]);
})(angular);
