//  Add User Form Model

(function (angular) {
    "use strict";

    var personaTypes = [{
            "name": "Production",
            "value": "1"
        }, {
            "name": "UAT",
            "value": "2"
        }];


    angular
        .module("settings")
        .value("personaTypeModel", personaTypes);
})(angular);
