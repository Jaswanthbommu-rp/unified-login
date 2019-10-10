
(function (angular) {
    "use strict";

    function factory() {
        var propertiesList = [{
            isSelected: true,
            propertyName: "Southwest apartments",
            city: "Dallas",
            state: "TX"
        }, {
            isSelected: true,
            propertyName: "Summer Hills",
            city: "Dallas",
            state: "TX"
        }, {
            isSelected: false,
            propertyName: "Hartford SQuare North",
            city: "Dallas",
            state: "TX"
        }, {
            isSelected: true,
            propertyName: "Lot A",
            city: "Dallas",
            state: "TX"
        }, {
            isSelected: false,
            propertyName: "Lot B",
            city: "Tulsa",
            state: "OK"
        }, {
            isSelected: false,
            propertyName: "Anthem",
            city: "Tulsa",
            state: "OK"
        }];

        return function() {
            return angular.copy(propertiesList);
        };

    }

    


    angular
        .module("settings")
        .factory("productPropertiesListData", factory);

})(angular);