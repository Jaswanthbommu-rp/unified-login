    //  Clone Role template Model

    (function(angular, undefined) {
        "use strict";

        function factory() {
            var model = {};

            model.init = function() {

                return model;
            };

            model.newRole = function() {
                return {
                    "roleId": 0,
                    "inheritRole": false,
                    "isSelected": false,
                    "role": "",
                    "rights": 0,
                    "users": 0,
                    "enterpriseRoles": 0,
                    "type": "Default",
                    "more": "more"
                };
            };


            return model.init();
        }

        angular
            .module("settings")
            .factory("onesiteCloneRoleTempModel", [
                factory
            ]);
    })(angular);