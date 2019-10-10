// User State

(function (angular) {
    "use strict";

    var userStates = {
        VIEW_USER: "view",
        EDIT_USER: "edit",
        ADD_USER: "add"
    };

    angular
        .module("settings")
        .value("userStates", userStates);

})(angular);
