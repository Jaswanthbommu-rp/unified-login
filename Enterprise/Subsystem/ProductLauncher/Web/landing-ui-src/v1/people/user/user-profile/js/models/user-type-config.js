//  User Type Select Menu Config

(function (angular, undefined) {
    "use strict";

    function UserTypeMenuConfig($filter, menuConfig, optionsSvc, userTypes) {
        var errorMsgs,
            index = 0,
            svc = this;

        errorMsgs = [
            {
                name: "required",
                text: $filter("userDetailsText")("err_user_type_required")
            }
        ];

        svc.get = function (data) {
            index++;

            data = angular.extend({
                required: true,
                nameKey: "name",
                valueKey: "partyRoleTypeId",
                errorMsgs: errorMsgs,
                id: "user-type-" + index,
                fieldName: "user-type",
                label: $filter("userDetailsText")("user_detail_user_type")
            }, data || {});

            var config = menuConfig(data);
            optionsSvc.get(svc.matchUserTypes.bind(null, config));
            return config;
        };

        svc.matchUserTypes = function(config, userTypes) {
            angular.forEach(userTypes, svc.matchUserType);
            config.setOptions(userTypes);
        };

        svc.matchUserType = function(ut) {
            switch(ut.partyRoleTypeId) {
                case userTypes.REGULAR.id: 
                    ut.name = userTypes.REGULAR.label;
                    break;
                case userTypes.RP_SYS_ADMIN.id: 
                    ut.name = userTypes.RP_SYS_ADMIN.label;
                    break;
                case userTypes.REGULAR_NO_EMAIL.id: 
                    ut.name = userTypes.REGULAR_NO_EMAIL.label;
                    break;
                case userTypes.SDE.id: 
                    ut.name = userTypes.SDE.label;
                    break;
                case userTypes.EXTERNAL_USER.id: 
                    ut.name = userTypes.EXTERNAL_USER.label;
                    break;
                case userTypes.RP_EMPLOYEE.id: 
                    ut.name = userTypes.RP_EMPLOYEE.label;
                    break;
            }
        };
    }

    angular
        .module("settings")
        .service("userTypeMenuConfig", [
            "$filter",
            "rpFormSelectMenuConfig",
            "userTypeOptions",
            "userTypes",
            UserTypeMenuConfig
        ]);
})(angular);
