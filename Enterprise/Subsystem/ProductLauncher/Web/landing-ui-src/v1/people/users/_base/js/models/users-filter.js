//  Add User Form Model

(function(angular) {
    "use strict";

    function factory($filter, baseForm, userModel) {
        var model = baseForm();

        model.getAccountStatus = function() {
            return [{
                label: $filter("userListText")("users_all"),
                value: ""
            }, {
                label: $filter("userListText")("users_active"),
                value: "1"
            }, {
                label: $filter("userListText")("users_disabled"),
                value: "24"
            }, {
                label: $filter("userListText")("users_expired"),
                value: "23"
            }, {
                label: $filter("userListText")("users_pending"),
                value: "2"
            }, {
                label: "Locked",
                value: "3"
            }];
        };

        model.getLockStatus = function() {
            return [{
                label: $filter("userListText")("users_all"),
                value: ""
            }, {
                label: $filter("userListText")("users_locked"),
                value: true
            }, {
                label: $filter("userListText")("users_unlocked"),
                value: false
            }];
        };

        model.setUserTypeOptions = function (options) {
            var optionsData = [];
            var userOption = {
                label: $filter("userListText")("users_all"),
                value: ""
            };

            optionsData.push(userOption);

            options.forEach(function (option) {
                var id = option.partyRoleTypeId;
                userOption = {};
                userOption.label = $filter("userListText")("options.userTypeId." + id);
                userOption.value = id;
                // if (id === 401){
                //     userOption.value = "user";
                // }
                // else if (id === 402){
                //     userOption.value = "superuser";
                // }
                // else if (id === 403){
                //     userOption.value = "realPage employee";
                // }
                // else if (id === 404){
                //     userOption.value = "user-noemail";
                // }

                optionsData.push(userOption);
            });
            optionsData = $filter("orderBy")(optionsData, "-label");

            return optionsData;
        };

        return model;
    }

    angular
        .module("settings")
        .factory("usersListFilterModel", [
            "$filter",
            "baseForm",
            "userSessionModel",
            factory
        ]);
})(angular);
