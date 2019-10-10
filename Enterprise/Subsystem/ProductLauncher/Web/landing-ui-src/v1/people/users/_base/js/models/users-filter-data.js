// User List Grid Filter

(function (angular) {
    "use strict";

    function factory() {
        return {
            name: "",
            product: "",
            property: "",
            userType: "",
            status: "",
            isLocked: ""            
        };
    }

    angular
        .module("settings")
        .factory("usersListFilterData", [factory]);
})(angular);
