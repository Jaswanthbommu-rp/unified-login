(function(angular, undefined) {
    "use strict";

    function factory(
        cloneTabsContext,
        $filter
    ) {
        var model = {};

        var cfg = {
            recordsPerPage: 10
        };

        model.init = function() {
            model.data = {
                roleName: "",
                cloneRoleName: "",
                cloneRoleDesc: ""
            };
            return model;
        };

        model.setRoleData = function() {
            model.data.roleName = cloneTabsContext.get().data.name ;
            model.data.cloneRoleName = cloneTabsContext.get().data.name + " (Copy)";
            model.data.cloneRoleDesc = cloneTabsContext.get().data.description ;
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

        model.reset = function() {
            model.data = {
                roleName: "",
                cloneRoleName: "'"
            };
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("spndmgmtCloneRoleModel", [            
            "spndmgmtCloneTabsContext",
            "$filter",
            factory
        ]);
})(angular);