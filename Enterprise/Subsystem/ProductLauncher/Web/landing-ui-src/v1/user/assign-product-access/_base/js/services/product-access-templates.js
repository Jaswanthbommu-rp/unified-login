//  Product Access Template Service

(function (angular, undefined) {
    "use strict";

    function ProductAccessTemplates(templatesData) {
        var svc = this;

        svc.getList = function () {
            var list = [];

            angular.forEach(templatesData, function (val, key) {
                var item = {
                    key: key
                };
                angular.extend(item, val);
                list.push(item);
            });

            return list;
        };
    }

    angular
        .module("settings")
        .service("productAccessTemplates", [
            "productAccessData",
            ProductAccessTemplates
        ]);
})(angular);
