//  Resident Portals Data Model

(function(angular, undefined) {
    "use strict";

    function factory() {
        function RPData() {
            var s = this;
            s.init();
        }

        var p = RPData.prototype;

        p.init = function() {
            var s = this;
            s.currentAdmin = false;
            s.propertiesReady = false;
            s.changed = false;
            s.active = false;
            s.tabsReady = false;
            s.data = {
                productId: 17, //TODO: Enum api for products instead of hard coded
                statusTypeId: 5,
                retryCount: 0,
                inputJson: {
                    roleList: [],
                    propertyList: [],
                    messageGroups: [],
                    Notifications: {
                        managerFdiViaEmail: false,
                        amenitiesViaEmail: false,
                        managerMrViaEmail: false,
                    },
                }
            };

            s.roles = [];
            s.properties = [];
            s.messageGroups = [];
            s._data = angular.copy(s.data);
        };

        p.setChanged = function() {
            var s = this;
            s.changed = true;
            return s;
        };

        p.hasChanged = function() {
            var s = this;
            return s.changed;
        };

        p.notificationsSet = function() {
            var s = this;
            return s.data.inputJson.Notifications.managerFdiViaEmail || s.data.inputJson.Notifications.amenitiesViaEmail || s.data.inputJson.Notifications.managerMrViaEmail;
        };

        p.setActive = function(bool) {
            var s = this;
            s.active = bool;
            return s;
        };

        p.isActive = function() {
            var s = this;
            return s.active;
        };

        p.setProperties = function(propertiesData) {
            var s = this;
            s.properties = propertiesData;
        };

        p.setRoles = function(rolesData) {
            var s = this;
            s.roles = rolesData;
        };

        p.setMessageGroups = function(messageGroupsData) {
            var s = this;
            s.messageGroups = messageGroupsData;
        };

        p.setFrontDesk = function(val) {
            var s = this;
            s.data.inputJson.Notifications.managerFdiViaEmail = val;
        };

        p.setAmenity = function(val) {
            var s = this;
            s.data.inputJson.Notifications.amenitiesViaEmail = val;
        };

        p.setServiceReq = function(val) {
            var s = this;
            s.data.inputJson.Notifications.managerMrViaEmail = val;
        };

        p.getTabsReady = function() {
            var s = this;
            return s.tabsReady;
        };

        p.setTabsReady = function(val) {
            var s = this;
            s.tabsReady = val;
        };

        p.getData = function() {
            var s = this,
                hasRoles = false,
                hasProperties = false,
                hasMessagingGroups = false,
                messagingAndNotificationReqd = false;

            if (s.properties && s.properties.length) {
                s.data.inputJson.propertyList = [];

                if (s.properties[0] !== "all") {
                    s.properties.forEach(function(prop) {
                        if (prop.isAssigned) {
                            s.data.inputJson.propertyList.push(prop.id);
                        }
                    });
                } else {
                    s.data.inputJson.propertyList.push("all");
                }

                hasProperties = s.data.inputJson.propertyList.length > 0;
            }

            if (s.roles && s.roles.length) {
                s.data.inputJson.roleList = [];

                s.roles.forEach(function(role) {
                    if (role.isAssigned()) {
                        s.data.inputJson.roleList.push(role.getId());
                        if (role.isStaff()) {
                            messagingAndNotificationReqd = true;
                        }
                    }
                });

                hasRoles = s.data.inputJson.roleList.length > 0;
            }

            if (s.messageGroups && s.messageGroups.length) {
                s.data.inputJson.messageGroups = [];

                s.messageGroups.forEach(function(messageGroup) {
                    if (messageGroup.isAssigned) {
                        s.data.inputJson.messageGroups.push(messageGroup.id);
                    }
                });

                hasMessagingGroups = s.data.inputJson.messageGroups.length > 0;
            }

            if (hasRoles && hasProperties) {
                // Changing to messaging and notifications no longer required.
                // Leaving below code in for future use but commmented out.
                // if (messagingAndNotificationReqd) {
                //     if (s.notificationsSet() && hasMessagingGroups) {
                //         return s.data;
                //     }
                //     return null;
                // }
                return s.data;
            }

            return null;
        };

        p.reset = function() {
            var s = this;
            s.currentAdmin = false;
            s.propertiesReady = false;
            s.roles = [];
            s.active = false;
            s.changed = false;
            s.tabsReady = false;
            s.properties = [];
            s.messageGroups = [];
            s.data = angular.copy(s._data);
        };

        return new RPData();
    }

    angular
        .module("settings")
        .factory("residentPortalsDataModel", [factory]);
})(angular);
