//  rights SpendManagement Controller

(function(angular, undefined) {
    "use strict";

    function SpndMgmtRightsCtrl(
        $scope,
        $filter,
        model,
        spndmgmtRightsGridConfig,
        spndmgmtRightsGridActions,
        prodConfig,        
        pubsub,
        $timeout
    ) {

        var vm = this;

        vm.init = function() {
            vm.model = model;
            prodConfig.setMethodsSrc(vm);
                       
            vm.destWatch = $scope.$on("$destroy", vm.destroy);            

        };
        

        vm.assignRoles = function(record) {
            vm.model.assignRolestoRights(record);
        };
        

        vm.showIcon = function(val,item){                                 
            item.showHideIcon =  val === 0 ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide =  val === 0 ? 'show' : 'hide';
        };

        vm.showIconToggle = function(item){                                 
            item.showHideIcon = item.showHideIcon === 'fa-angle-down' ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide = item.showHide === 'hide' ? 'show' : 'hide';
            var i=0;
            item.subGroupList.forEach(function (subItem) {
                if(item.showHide === 'show'){
                    if(i===0){
                        subItem.showHideIcon1 = 'fa-angle-up';
                        subItem.showHide1 = 'show';
                    }else{
                        subItem.showHideIcon1 = 'fa-angle-down';
                        subItem.showHide1 = 'hide';
                    }

                }else{
                     subItem.showHideIcon1 = item.showHideIcon;
                     subItem.showHide1 = item.showHide;
                }
               
                i++;
            });
        };

        vm.showIconToggle1 = function(item){            
            item.showHideIcon1 = item.showHideIcon1 === 'fa-angle-down' ? 'fa-angle-up' : 'fa-angle-down';
            item.showHide1 = item.showHide1 === 'hide' ? 'show' : 'hide';            
        };


        vm.showIcon1 = function(val, subval1){            
            var flag = false;
            if(val === 0 && subval1 === 0 ){                
                flag = true;
            }
            
            return flag === true ? 'fa-angle-up' : 'fa-angle-down';
        };

        vm.showRow = function(val, subval1, subval2){
            
            var flag = false;
            if(val === 0 && subval1 === 0 ){                
                flag = true;
            }

            return flag === true ? 'show' : 'hide';
        };

        vm.showRow1 = function(val, subval1){            
            var flag = false;
            if(val === 0  ){                
                flag = true;
            }

            return flag === true ? 'show' : 'hide';
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
        .controller("SpndMgmtRightsCtrl", [
            "$scope",
            "$filter",
            "rolAndRhtSpndMgmtRightsModel",
            "spndmgmtRightsGridConfig",
            "spndmgmtightsGridActions",
            "spndmgmtProductsConfig",            
            "pubsub",
            "$timeout",
            SpndMgmtRightsCtrl
        ]);
})(angular);