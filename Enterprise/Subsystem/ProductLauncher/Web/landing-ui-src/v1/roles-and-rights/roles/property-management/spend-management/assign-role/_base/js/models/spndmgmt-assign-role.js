(function(angular, undefined) {
    "use strict";

    function factory(

        tabsContext,
        $filter,
        roleConfig,
        security
    ) {
        var model = {};

        model.init = function() {
            model.roleData = {};
            model.assignRoleName = "";
            model.assignRoleDesc = "";
            model.currentType = "";
            return model;
        };

        model.setRoleData = function() {            
            model.roleData = tabsContext.get().data;            
            model.assignRoleName = angular.copy(model.roleData.name);
            model.assignRoleDesc = angular.copy(model.roleData.description);

            if (tabsContext.get().type === "edit") {
                model.currentType = "edit";                
                model.headerText = "Edit Role";
                // roleConfig.assignRoleName.readonly = true;
                // roleConfig.assignRoleDesc.readonly = true;                
                
            }else if (tabsContext.get().type === "view" && model.hasViewAccess()) {
                model.headerText = "View Role";
            } 
            else {
                model.headerText = "Assign Role";
                //roleConfig.assignRoleName.readonly = true;
            }
            return model;
        };

        model.getDataSvc = function(dataSvcGet) {
            var params = {};
            dataSvcGet.getData(params)
                .then(model.setDataFromSvc, model.setDataErr);
        };

        model.setData = function(data) {
            model.data = data;
        };

        model.getData = function() {
            return model.data;
        };

        model.setDataFromSvc = function(data) {
            model.setData(data);
        };

        model.setDataErr = function(data) {
            logc("Error = > ", data);
        };

        model.hasViewAccess = function () {
            return security.isAllowed("viewRoleRight");
        };   

        model.reset = function() {
            model.headerText = "";
            model.assignRoleName = "";
            model.assignRoleDesc = "";
            model.currentType = "";
            model.roleData = {};
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("spndmgmtAssignRoleModel", [            
            "spndmgmtAssignTabsContext",
            "$filter",
            "spndmgmtAssignRoleFormConfig",
            "routeSecurity",
            factory
        ]);
})(angular);