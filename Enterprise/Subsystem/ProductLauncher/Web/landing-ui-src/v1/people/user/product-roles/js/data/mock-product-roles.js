
(function (angular) {
    "use strict";

    function factory() {

        var rolesList = [{
            isSelected: true,
            role: "AB Integration",
            roleType: "Custom"
        }, {
            isSelected: false,
            role: "Accountant",
            roleType: "Default"
        }, {
            isSelected: false,
            role: "Supervisor",
            roleType: "Default"
        }, {
            isSelected: true,
            role: "Superman",
            roleType: "Custom"
        }, {
            isSelected: true,
            role: "Superheroes",
            roleType: "Custom"
        }, {
            isSelected: false,
            role: "Super",
            roleType: "Default"
        }];

        return function() {
            return angular.copy(rolesList);
        };

    }

    


    angular
        .module("settings")
        .factory("productRolesListData", factory);

})(angular);