//  user-mgmt rights  Controller

(function(angular, undefined) {
    "use strict";

    function UserMgmtRightsCtrl(
        $scope,
        $filter,
        model,
        rightsGridConfig,
        rightsGridActions,
        prodConfig,        
        pubsub,
        $timeout
    ) {

        var vm = this;

        vm.init = function() {

            rightsGridConfig.setSrc(vm);
            rightsGridActions.setSrc(vm);

            vm.model = model;

            prodConfig.setMethodsSrc(vm);

            vm.prodConfig = prodConfig;
            
            vm.model.initWatch();
            vm.model.initGrid();
            vm.destWatch = $scope.$on("$destroy", vm.destroy);          

        };

        
        vm.assignRoles = function(record) {
            vm.model.assignRolestoRights(record);
        };

        vm.destroy = function() {
            vm.destWatch();            
            vm.model.reset();
            vm = undefined;
        };


        vm.init();
    }

    angular
        .module("settings")
        .controller("UserMgmtRightsCtrl", [
            "$scope",
            "$filter",
            "userMgmtRightsModel",
            "userMgmtRightsGridConfig",
            "userMgmtRightsGridActions",
            "userMgmtProductsConfig",            
            "pubsub",
            "$timeout",
            UserMgmtRightsCtrl
        ]);
})(angular);