// 

(function (angular, undefined) {
    "use strict";

    function CompSwitchModalCtrl($scope, $location, modal, menuConfig, compMenu, pubsub, persona) {
        var vm = this;

        vm.init = function () {
            menuConfig.setMethodsSrc(vm);

            vm.menuConfig = menuConfig;
            vm.personaWatch = angular.noop;

            if (persona.isReady()) {
                compMenu.getCompData();
            } else {                
                vm.personaWatch = persona.subscribe(compMenu.getCompData);
            }            

            vm.smWatch = pubsub.subscribe("compSwitch.setSelMenu", vm.setSelMenu);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.dismissModal = function () {
            modal.hide();           
        };

        vm.switchCompany = function () {               
            var compId = compMenu.getSelCompId();
            if(compId === persona.getOrgRealPageID()){                
                return false;
            }
            modal.hide(); 
            
            var newPersonaid = compMenu.getSelComp(compId)[0].personas[0].personaId;
            var redirectUrl = "../access/" + persona.getPersonaRealPageID() + "|" + newPersonaid;            
            window.location.href  = redirectUrl ;
        };

        vm.setMenuData = function() {
            menuConfig.setOptionsFilter("optionsData", vm.optionsFilterVal);
            vm.filterMenuVal = compMenu.getSelCompId();
        };


        vm.setSelMenu = function() {
            vm.filterMenuVal = compMenu.getSelCompId();
        };


        vm.menuChange = function(value) {            
            compMenu.setSelCompId(value);
            pubsub.publish("compswitch.compChange");
        };

        vm.destroy = function () {
            vm.destWatch();
            vm.personaWatch();
            vm.smWatch();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("CompSwitchModalCtrl", [
            "$scope",
            "$location",
            "compSwitchModal",
            "compMenuConfig",
            "compSwitchSelectMenuModel",
            "pubsub",
            "personaDetails",
            CompSwitchModalCtrl
        ]);
})(angular);
