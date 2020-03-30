//  VendCompProductAccess Controller

(function(angular, undefined) {
    "use strict";

    function ClientPortalProductAccessCtrl($scope, $filter, tabsMenu, tabsData, ClientPortalProductModel, pubsub, jsonData, configData, configFactory, configModel) {
        var vm = this, tabsCnfData = [], gridconfigs = [], radioconfigs = [];

        vm.init = function() {
            vm.panelName = $filter("productPanelText")("panelName.clientPortal");
            vm.data = jsonData;
            // console.log('MAIN');
            // tabsCnfData = vm.getTabsConfigData(vm.data);
            // gridconfigs = vm.getGridConfigs(tabsCnfData);
            // configModel.setGridConfig(gridconfigs);

            // radioconfigs = configData.getRadioConfig(vm.data);
            // logc("cnfg for radio" ,radioconfigs);

            // configModel.setRadioConfig(radioconfigs);

            vm.productDisabled = false;
            vm.productAccessWatch = pubsub.subscribe("pa.regUserNoEmailNotAllowed", vm.setProductDisabled);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);
        };


        vm.getTabsConfigData = function (jsonData) {

            var cnfg = {}, tabs = [];

            if(jsonData && jsonData.Tabs){
                jsonData.Tabs.forEach(function (data) {
                    data.Controls.forEach(function (ctrl) {
                        if(ctrl.Type === 'Select Grid' ){
                            cnfg = configData.getGridConfig(ctrl);
                        }
                    });
                    // logc("cnfg for " + data.DisplayName, cnfg);
                    tabs.push(cnfg);
                });
            }

            logc("tabs for ", tabs);
            return tabs;
        };

        vm.getGridConfigs = function (tabsCfData) {

            var cnfgs = [];

            if(tabsCfData){
                tabsCfData.forEach(function (tab) {

                    var hdrCnfgs = {} , fltrCnfg = {}, mainCnfg = {} ;

                    var h = configData.getHeaders(tab);
                    hdrCnfgs = h;

                    var f = configData.getFilters(tab);
                    fltrCnfg = f;

                    var m = configData.getFilters(tab);
                    mainCnfg = m;

                    var cnfg = {
                        "headers" : hdrCnfgs,
                        "filters" : fltrCnfg,
                        "main"    : mainCnfg
                    };

                    var c = configFactory(cnfg);
                    cnfgs.push(c);

                });
            }


            logc("cnfg for ", cnfgs);
            return cnfgs;
        };



        vm.isActive = function () {
            return ClientPortalProductModel.isActive();
        };

        vm.setChanged = function () {
            ClientPortalProductModel.setChanged();
        };

        vm.setProductDisabled = function (value) {
            vm.productDisabled = value;
        };

        vm.destroy = function() {
            vm.destWatch();
            vm.productAccessWatch();
            vm.productDisabled = undefined;
            tabsData.reset();
            ClientPortalProductModel.reset();
            vm = undefined;
            $scope = undefined;
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("ClientPortalProductAccessCtrl", [
            "$scope",
            "$filter",
            "clientPortalTabsMenu",
            "clientPortalTabsData",
            "clientPortalDataModel",
            "pubsub",
            "DataModel",
            "configDataModel",
            "gridConfigFactory",
            "ConfigModel",
            ClientPortalProductAccessCtrl
        ]);
})(angular);
