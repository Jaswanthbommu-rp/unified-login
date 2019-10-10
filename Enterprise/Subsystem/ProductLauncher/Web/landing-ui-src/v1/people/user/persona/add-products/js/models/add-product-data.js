//  Persona - Add Product Form Data Model

(function (angular) {
    "use strict";

    function factory() {
        var model = {};

        model = {
            searchObj: {
                model: {
                    name: ""                    
                }
            },
            selectAll: false,
            productList: [], //main source of products
            families: [] //list of products sorted for view
        };

        model.clearData = function () {
            model.searchObj.model.name = "";
        };

        return model;
    }

    angular
        .module("settings")
        .factory("addProductFormData", [factory]);
})(angular);
