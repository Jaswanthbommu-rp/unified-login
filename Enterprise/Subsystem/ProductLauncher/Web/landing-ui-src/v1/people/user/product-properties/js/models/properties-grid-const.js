// Product Item Properties Access Grid Constants

(function (angular) {
    "use strict";

    function factory() {
        var model = {

            isSelected: {
                id: "isSelected",
                isSortable: false,
                isEnabled: true,
                type: "select"
            },
            property: {
                id: "propertyName",
                text: "Property",
                isSortable: true,
                type: "text"
            },
            city: {
                id: "city",
                text: "City",
                isSortable: true,
                type: "text"
            },
            state: {
                id: "state",
                text: "State",
                isSortable: true,
                type: "text"
            }

        };

        return model;
    }

    angular
        .module("settings")
        .factory("productPropertiesGridConst", [
            factory
        ]);
})(angular);
