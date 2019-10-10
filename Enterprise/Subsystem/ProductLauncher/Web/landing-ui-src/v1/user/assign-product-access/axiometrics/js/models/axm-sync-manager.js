//  Sync Manager Model

(function (angular, undefined) {
    "use strict";

    function factory(pubsub) {
        function AXMSyncManager() {
            var s = this;
            s.init();
        }

        var p = AXMSyncManager.prototype;

        p.init = function () {
            var s = this;

            s.groupMap = {};
            s.propertyMap = {};
            s.roleMap = {};
            s.companyGroupMap = {};

            s.groupList = [];
            s.propertyList = [];
            s.roleList = [];
            s.companyGroupList = [];
        };

        // Getters

        p.getSelectedCount = function (list, selectKey) {
            var s = this,
                count = 0;

            list.forEach(function (item) {
                if (item[selectKey]) {
                    count++;
                }
            });

            return count;
        };

        // Setters

        p.setGroupList = function (list) {
            var s = this;
            s.groupList = list;
            s.renderMap();
            return s;
        };


        p.setGroupSelectKey = function (key) {
            var s = this;
            s.groupSelectKey = key;
            return s;
        };

        p.setPropertyList = function (list) {
            var s = this;
            s.propertyList = list;
            s.renderMap();
            return s;
        };

        p.setPropertySelectKey = function (key) {
            var s = this;
            s.propertySelectKey = key;
            return s;
        };

        p.setRoleList = function (list) {
            var s = this;
            s.roleList = list;
            s.renderMap();
            return s;
        };

        p.setRoleSelectKey = function (key) {
            var s = this;
            s.roleSelectKey = key;
            return s;
        };
        // Actions

        p.selectedPropertySync = function (property) {
            var s = this,
                propertyList,
                selectState = false;

            propertyList = s.propertyMap['company' + property.companyId].property;

            propertyList.properties.forEach(function (item) {
                if (item.companyId === property.propertyId) {
                    item["isAssigned"] = property.isAssigned;
                }

            });
            return s;
        };

        p.updatePropertyList = function (property) {
            var s = this;
            s.propertyList.push(property);
            return s;
        };

        p.getTranslatedRoleList = function (key) {
            var s = this,
                roleList,
                data,
                option,
                selectState = false;

            s.data = {
                title: "Select All Roles",
                options: []
            };

            roleList = s.roleMap['company' + key].role;

            roleList.forEach(function (data) {
                s.option = {
                    name: data.name,
                    isAssigned: data.isAssigned,
                    disabled: false
                };

                s.data.options.push(s.option);
            });

            return s.data;
        };

        p.selectedRoleSync = function (key, option) {
            var s = this,
                roleData,
                selectedRole,
                i = 0,
                selectState = false;

            roleData = s.roleMap['company' + key].role;

            roleData.forEach(function (item) {
                if (item.name === option.name) {
                    item.isAssigned = option.isAssigned;
                }
                if (item.isAssigned) {
                    i++;
                }
            });

            if (i > 0) {
                selectState = true;
            }

            s.roleList.forEach(function (item) {
                if (item.companyId === key) {
                    item["isAssigned"] = selectState;
                    selectedRole = item;
                }

            });

            return s;
        };

        p.renderMap = function () {
            var s = this;

            if (!s.roleList.empty()) {
                s.roleList.forEach(function (role) {
                    s.roleMap['company' + role.companyId] = {
                        role: role.roles,
                        roles: []
                    };
                });
            }

            if (!s.groupList.empty()) {
                s.groupList.forEach(function (group) {
                    s.groupMap['company' + group.companyId] = {
                        group: group,
                        groups: []
                    };
                });
            }

            if (!s.propertyList.empty()) {
                s.propertyList.forEach(function (property) {
                    s.propertyMap['company' + property.companyId] = {
                        property: property,
                        properties: []
                    };
                });
            }

        };

        p.updateSelectState = function (list, selectKey, bool) {
            var s = this;

            list.forEach(function (item) {
                item[selectKey] = bool;
            });

            return s;
        };

        // Assertions

        p.allSelected = function (list, selectKey) {
            var s = this;
            return s.getSelectedCount(list) === list.length;
        };

        p.reset = function () {
            var s = this;
            s.groupMap = {};
            s.companyGroupMap = {};
            s.propertyMap = {};
            s.roleMap = {};
            s.groupList = [];
            s.propertyList = [];
            s.roleList = [];
            s.companyGroupList = [];
        };

        return new AXMSyncManager();
    }

    angular
        .module("settings")
        .factory("axmSyncManager", [
            "pubsub",
            factory
        ]);
})(angular);
