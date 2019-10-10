//  Rights  Controller

(function(angular, undefined) {
    "use strict";

    function RightsController(
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
            model.getRightsSideMenu();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.selectSoln = function (soln) {  
            productsModel.setFamilyAccordian(soln); 
            productsModel.setActiveSol(soln);                  
            model.setActiveSol(soln);            
        };
      
        vm.destroy = function() {
            vm.destWatch();
            vm = undefined;
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
        .controller("RightsController", [
            "$scope",
            "$filter",
            "rolesAndRightsProductsModel",            
            "rightsMenuModel",
            RightsController
        ]);
})(angular);