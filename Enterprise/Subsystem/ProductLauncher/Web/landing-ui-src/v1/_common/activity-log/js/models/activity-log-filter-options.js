//  Add User Form Model

(function (angular) {
    "use strict";

    function factory($filter, baseForm) {
        var model = baseForm();

        model.getActivityTypes = function() {
            return [{
                label: "All Activities",
                value: ""
            },{
                label: "Change password failure",
                value: "4"
            }, {
                label: "Change password success",
                value: "3"
            }, {
                label: "Change security questions success",
                value: "63"
            }, {
                label: "Create user",
                value: "7"
            }, {
                label: "Email sent",
                value: "14"
            }, {
                label: "Email resent",
                value: "15"
            }, {
                label: "Login enabled",
                value: "10"
            }, {
                label: "Login disabled",
                value: "11"
            }, {
                label: "Login failure",
                value: "2"
            }, {
                label: "Login success",
                value: "1"
            }, {
                label: "Product Access",
                value: "17"
            }, {
                label: "Signout",
                value: "16"
            }, {
                label: "User Locked",
                value: "12"
            }, {
                label: "User Unlocked",
                value: "13"
            }, {
                label: "User Update",
                value: "8"
            }];
        };

        model.getDateRanges = function() {
            return [{
                label: "Current month",
                value: "1-CM"
            },{
                label: "Last calendar week",
                value: "1-w"
            }, {
                label: "Year to date",
                value: "1-ytd"
            }, {
                label: "Last 180 days",
                value: "180-d"
            }, {
                label: "Last 30 days",
                value: "30-d"
            }, {
                label: "Last 14 days",
                value: "14-d"
            }, {
                label: "Last 7 Days",
                value: "7-d"
            },{
                label: "Yesterday",
                value: "1-yday"
            }, {
                label: "Today",
                value: "0-d"
            // }, {
            //     label: "Custom",
            //     value: "Custom"
            }];
        };

        model.getSortFilters = function() {
            return [{
                label: "Most Recent",
                value: "ApplicationTimeStamp-DESC"
            },{
                label: "Activity (A to Z)",
                value: "LogTypeName-ASC"
            }, {
                label: "Activity (Z to A)",
                value: "LogTypeName-DESC"
            // }, {
            //     label: "User (A to Z)",
            //     value: "user-ASC"
            // }, {
            //     label: "User (Z to A)",
            //     value: "user-DESC"
            // }, {
            //     label: "Property (A to Z)",
            //     value: "property-ASC"
            // }, {
            //     label: "Property (Z to A)",
            //     value: "property-DESC"
            }];
        };

        return model;
    }

    angular
        .module("settings")
        .factory("activityLogFilterOptions", [
            "$filter",
            "baseForm",
            factory
        ]);
})(angular);
