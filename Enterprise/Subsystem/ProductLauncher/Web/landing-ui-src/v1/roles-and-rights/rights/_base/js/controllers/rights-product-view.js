//  Rights Product Access  Controller

(function(angular, undefined) {
    "use strict";

    function RightsAssignProductAccessCtrl(
        $scope,
        $filter,
        model,
        productsModel
    ) {

        var vm = this;
        vm.init = function() {
            vm.active = {};
            vm.list = model.getSidemenuList();
            if (vm.list !== undefined && vm.list.length > 0) {
                var sol = productsModel.getActiveSol();
                if (sol === "") {
                    vm.list[0].active = true;
                    vm.list[0].solution.selected = true;
                } else {
                    vm.list.forEach(function(item) {
                        item.active = false;
                        item.solution.selected = false;
                        if (sol.solutionId === item.solutionId) {
                            item.active = true;
                            item.solution.selected = true;
                        }
                    });
                }
            }
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm = undefined;
            $scope = undefined;
        };


        vm.init();
    }

    angular
        .module("settings")
        .controller("RightsAssignProductAccessCtrl", [
            "$scope",
            "$filter",
            "rightsMenuModel",
            "rolesAndRightsProductsModel",
            RightsAssignProductAccessCtrl
        ]);
})(angular);