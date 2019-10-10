// User List Grid config factory

(function (angular) {
    "use strict";

    function factory(rpGridConfig) {
        return function(data){

            var model = rpGridConfig();

            model.get = function () {
                return data.getKeys;
            };

            model.getHeaders = function () {
                return data.getHeaders;
            };

            model.getFilters = function () {
                return data.getFilters;
            };

            return model;
        };
    }

    angular
        .module("settings")
        .factory("companyGridConfigFactory", [ "rpGridConfig" , factory]);
})(angular);
