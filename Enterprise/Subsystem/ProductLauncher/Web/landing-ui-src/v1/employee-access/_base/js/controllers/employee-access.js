//  EmployeeAccessCtrl Controller

(function(angular, undefined) {
    "use strict";

    function EmployeeAccessCtrl(
        $scope,
        frmCnfg,
        model,
        gridConfig,
        gridActions,
        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        menuConfig,        
        menuData,
        userModel,
        userGridConfig,
        gridUserActions
    ) {
        var vm = this,
        showList = {
            compView : true,
            userView : false
        };

        vm.init = function() {
            vm.defaultMenuSelected = "Company";
            vm.showCompany = true;
            vm.showList = showList;
            gridConfig.setSrc(vm);
            gridActions.setSrc(vm);
            gridUserActions.setSrc(vm);
            userGridConfig.setSrc(vm);
            
            menuConfig.setMethodsSrc(vm);
            vm.menuConfig = menuConfig;
            vm.model = model;
            vm.userModel = userModel;
            frmCnfg.setMethodsSrc(vm);
            vm.config = frmCnfg;
            vm.setMenu();
            vm.model.initGrid();
            vm.userModel.initGrid();

            vm.formWatch = $scope.$watch("searchUserForm", vm.setForm);
            vm.destWatch = $scope.$on("$destroy", vm.destroy);

        };

        vm.destroy = function() {
            vm.destWatch();
            vm.formWatch();
            vm = undefined;
        };

        vm.filterInput = function(inp) {
            inp = inp.toLowerCase();
            model.searchFilter(inp);
        };

        vm.filterInputUser = function(inp) {
            if(inp !== undefined && inp.length >= 5){
                userModel.setBusyGrid();                
                userModel.getDataSvc(inp);
            }else{                
                 userModel.setFilteredDataToGrid([]);
            }
        };

        vm.setForm = function(form) {
            if (form) {
                vm.form = form;
                vm.formWatch();
            }
        };
        

        vm.setMenu = function() {
            menuConfig.setOptions("optionsData", menuData.getMenuData());                  
        };


        vm.menuChange = function(value) {            
            vm.showList.compView = "Company" === value;
            vm.showList.userView = "User" === value;
        };

        vm.checkChars = function (data) {
            if(data === undefined){
                return;
            }
                 
            var val = (data.length < 5);
            
            if(val && data.length !== 0){                
                vm.searchForm.$setSubmitted();
            }
            return (!val);
        };

        vm.init();
    }

    angular
        .module("settings")
        .controller("EmployeeAccessCtrl", [
            "$scope",
            "empAccessFormConfig",
            "empAccessModel",
            "empAccessCompGridConfig",
            "empAccessGridActions",
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "empAccessMenuConfig",            
            "empAccessMenuData",
            "empAccessUserModel",
            "empAccessUserGridConfig",
            "empAccessUsersGridActions",
            EmployeeAccessCtrl
        ]);
})(angular);