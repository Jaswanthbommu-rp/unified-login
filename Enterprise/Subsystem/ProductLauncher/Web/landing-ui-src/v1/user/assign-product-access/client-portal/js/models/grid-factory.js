//  Grid config factory

(function (angular) {
    "use strict";

    function factory(rpGridConfig) {
        return function(data){

            var model = rpGridConfig();

            model.get = function () {
                return data.main;
            };

            model.getHeaders = function () {
                return data.headers;
            };

            model.getFilters = function () {
                return data.filters;
            };

            return model;
        };
    }

    angular
        .module("settings")
        .factory("gridConfigFactory", [ "rpGridConfig" , factory]);
})(angular);
