//  Sample Select Menu Form Data

(function (angular) {
    "use strict";

    function factory() {
        return {
            searchText: "",
            prodFamily: "",
            prodSolution: ""
        };
    }

    angular
        .module("settings")
        .factory("productsFilterForm", [factory]);
})(angular);
