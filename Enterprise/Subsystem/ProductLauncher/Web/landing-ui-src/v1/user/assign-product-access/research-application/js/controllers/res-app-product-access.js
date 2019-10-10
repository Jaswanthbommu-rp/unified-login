//  res-app ProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function ResAppProductAccess($scope,         
        tabsModel, 
        tabsData, 
        raDataModel) {
        var vm = this,
        panelName = "ResApp";

        vm.init = function() {
            vm.panelName = panelName;
            vm.model = raDataModel;
                        
            var tabs = ["roles", "goals"];
            
            tabsModel.setTabs(tabs);
            vm.tabsList = tabsModel.getTabsList();
            vm.tabsMenu = tabsModel.getTabsMenu();
            
            vm.changeWatch = vm.tabsMenu.subscribe("change", vm.onChange);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };

        vm.onChange = function (tab) {
            logc("tab", tab);
        };

        vm.isActive = function() {
            return raDataModel.isActive();
        };

        vm.setChanged = function() {
            raDataModel.setChanged();
        };

        vm.destroy = function() {            
            vm.changeWatch();
            tabsData.reset();
            vm.destWatch();
            vm = undefined;
            $scope = undefined;           
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ResAppProductAccess", [
            "$scope",            
            "resAppTabsMenuModel",
            "resAppTabsData",
            "resAppDataModel",
            ResAppProductAccess
        ]);
})(angular);