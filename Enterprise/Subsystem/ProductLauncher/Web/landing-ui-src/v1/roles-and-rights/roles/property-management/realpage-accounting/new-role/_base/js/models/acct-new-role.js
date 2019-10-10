(function(angular, undefined) {
    "use strict";

    function factory(

        gridModel,
        gridTransformSvc,
        gridPaginationModel,
        $filter
    ) {
        var model = {};

        var cfg = {
            recordsPerPage: 10
        };

        model.init = function() {
            model.data = {
                roleName: ""
            };
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
            model.data.roleName = "";
        };

        return model.init();
    }

    angular
        .module("settings")
        .factory("acctNewRoleModel", [
            "rpGridModel",
            "rpGridTransform",
            "rpGridPaginationModel",
            "$filter",
            factory
        ]);
})(angular);