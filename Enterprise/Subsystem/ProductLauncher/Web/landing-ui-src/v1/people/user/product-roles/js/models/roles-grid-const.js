// Product Item Roles Access Grid Constants

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
            role: {
                id: "role",
                text: "Role",
                isSortable: true,
                type: "text"
            },  
            roleType: {
                id: "roleType",
                text: "Role Type",
                isSortable: true,
                type: "text"
            }

        };

        return model;
    }

    angular
        .module("settings")
        .factory("productRolesGridConst", [
            factory
        ]);
})(angular);
