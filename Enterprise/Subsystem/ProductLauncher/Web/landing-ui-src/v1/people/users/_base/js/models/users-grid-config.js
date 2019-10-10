// Users List Grid Configuration

(function (angular) {
    "use strict";

    function gridFactory($filter, rpGridConfig, usersGridActions) {
        var gridConfig = rpGridConfig();

        gridConfig.keys = {
            isSelected: "isSelected",
            username: "user",
            products: "productCount",
            properties: "propertyCount",
            lastLogin: "lastLogin",
            accountStatus: "accountStatus",
            lockStatus: "lockStatus",
            actions: "actions",
            class: "accountStatusClass",
            profileLink: "userProfileLink",
            customFieldCol: "customFieldCol"
            
        };

        //define colummn config
        gridConfig.get = function() {
            return [{
                idKey: "isSelected",
                key: gridConfig.keys.isSelected,
                type: "select"
            }, {
                key: gridConfig.keys.username,
                type: "custom",
                profileLink: gridConfig.keys.profileLink,
                templateUrl: "people/users/base/templates/grid-username.html"
            },
            {
                key: gridConfig.keys.customFieldCol                    
            },
            {
                key: gridConfig.keys.products,
                type: "custom",
                templateUrl: "people/users/base/templates/grid-number.html"
            }, {
            //     key: gridConfig.keys.properties,
            //     type: "custom",
            //     templateUrl: "people/users/base/templates/grid-number.html"
            // }, {
                key: gridConfig.keys.lastLogin,
                type: "custom",
                templateUrl: "people/users/base/templates/grid-last-login.html"
            }, {
                key: gridConfig.keys.accountStatus,
                type: "custom",
                //statusClass: gridConfig.keys.accountStatus,
                templateUrl: "people/users/base/templates/grid-acct-status.html"
            }, {
                key: gridConfig.keys.lockStatus,
                type: "custom",
                templateUrl: "people/users/base/templates/grid-lock-status.html"
            }, {
                key: gridConfig.keys.actions,
                type: "actionsMenu",
                getActions: usersGridActions.get
            }];
        };

        //define header config
        gridConfig.getHeaders = function() {
            return [[{
                key: gridConfig.keys.isSelected,
                text: "",
                enabled: true
            }, {
                key: gridConfig.keys.username,
                text: $filter("userListText")("users_user_col"),
                isSortable: true
            }, 
            {
                key: gridConfig.keys.customFieldCol ,
                text : "",
            },
            {
                key: gridConfig.keys.products,
                text: $filter("userListText")("users_products_col"),
                isSortable: true
            }, {
            //     key: gridConfig.keys.properties,
            //     text: $filter("userListText")("users_properties_col"),
            //     isSortable: true
            // }, {
                key: gridConfig.keys.lastLogin,
                text: $filter("userListText")("users_last_login_col"),
                isSortable: true
            }, {
                key: gridConfig.keys.accountStatus,
                text: $filter("userListText")("users_status_col"),
                isSortable: true
            }, {
                key: gridConfig.keys.lockStatus,
                text: "",
                isSortable: false
            }, {
                key: gridConfig.keys.actions,
                text: $filter("userListText")("users_action_col"),
                isSortable: false
            }]];
        };

        return gridConfig;
    }

    angular
        .module("settings")
        .factory("userListGridConfig", [
                "$filter",
                "rpGridConfig",
                "userListGridActions",
                gridFactory
        ]);

})(angular);
