//  Roles  Controller

(function(angular, undefined) {
    "use strict";

    function RolesController(
        $scope,
        $filter,
        productsModel,
        model
    ) {

        var vm = this;
        vm.init = function() {
            vm.families = productsModel.getFamilies();
            vm.setInitialFamActive(vm.families);
            model.setFamilies(vm.families);
            model.getRolesSideMenu();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.destroy = function() {
            vm.destWatch();
            vm = undefined;
        };

        vm.selectSoln = function(soln) {
            productsModel.setFamilyAccordian(soln);
            productsModel.setActiveSol(soln);
            model.setActiveSol(soln);
        };

        vm.setInitialFamActive = function(families) {
            var solAct = productsModel.getActiveSol();
            if (solAct === "") {
                var i = 0;
                families.forEach(function(fam) {
                    fam.isActive = false;
                    if (i === 0) {
                        fam.isActive = true;
                    }
                    i++;
                });
            }
        };



        vm.init();
    }

    angular
        .module("settings")
        .controller("RolesController", [
            "$scope",
            "$filter",
            "rolesAndRightsProductsModel",
            "rolesMenuModel",
            RolesController
        ]);
})(angular);